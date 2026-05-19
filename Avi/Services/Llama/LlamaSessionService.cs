using Avi.Audio;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLama.Transformers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Avi.Services.AI
{
    public class LlamaSessionService : ISpeechListener, IDisposable
    {
        private readonly LlamaExecutorService _executorService;
        private readonly ChatHistoryManager _historyManager;
        private SpeechRecognitionServer _speechServer;
        private readonly ISettingsService _settings;

        private ChatSession? _chatSession;

        private bool _canceled;
        private string _fullPrompt = "";

        // Kolejka wiadomości
        private readonly Channel<(string Message, AuthorRole Role)> _messageQueue;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // Zabezpieczenie: pętla kolejki poczeka, aż wywołasz LoadHistory()
        private readonly TaskCompletionSource _initLatch = new();

        // Event do MainWindow
        public event Action<string>? OnTokenGenerated;

        public LlamaSessionService(SpeechRecognitionServer speechServer, ISettingsService settings)
        {
            _executorService = new LlamaExecutorService(settings);
            _historyManager = new ChatHistoryManager();
            _speechServer = speechServer;
            _speechServer.ServiceUsers.Add(this);
            _settings = settings;

            // Inicjalizacja kolejki
            _messageQueue = Channel.CreateUnbounded<(string, AuthorRole)>();

            // Odpalenie pętli w tle z zabezpieczeniem przed wątkiem UI
            _ = Task.Run(() => ProcessQueueAsync(_cancellationTokenSource.Token));
        }

        public async Task Load()
        {
            
            await _executorService.LoadModel();
            await _historyManager.InitializeAsync();
        }

        public async Task LoadHistory()
        {
            await _historyManager.GenerateAndSaveAISummaryAsync(_executorService.Weights, _executorService.ModelParams);
            _historyManager.LoadHistory();
            _chatSession = _executorService.CreateChatSession(_historyManager.History);

            string historyString = _historyManager.History.ToJson();
            Debug.WriteLine($"[DEBUG] History Loaded: {historyString}");

            // Odblokowujemy przetwarzanie kolejki, bo sesja jest gotowa!
            _initLatch.TrySetResult();
        }

        // POPRAWIONE: Jesteś zainteresowany, jeśli sesja czatu jest gotowa do pracy
        public bool IsInterested(string audioTranscription)
            => _chatSession != null || audioTranscription.Contains("stop", StringComparison.CurrentCultureIgnoreCase);

        public void HandleSpeech(string audioTranscription)
        {
            if (audioTranscription.Contains("stop", StringComparison.CurrentCultureIgnoreCase))
            {
                Cancel();
            }
            else
            {
                Debug.WriteLine($"[DEBUG] HandleSpeech pushing to queue: {audioTranscription}");
                _messageQueue.Writer.TryWrite((audioTranscription, AuthorRole.User));
            }
        }

        public async Task OnSpeechRecognizedAsync(string text)
        {
            Debug.WriteLine($"[DEBUG] OnSpeechRecognizedAsync pushing to queue: {text}");
            _messageQueue.Writer.TryWrite((text, AuthorRole.User));
        }

        public async Task SendMessageAsync(string message, AuthorRole authorRole)
        {
            Debug.WriteLine($"[DEBUG] SendMessageAsync pushing to queue: {message}");
            _messageQueue.Writer.TryWrite((message, authorRole));
        }

        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("[DEBUG] ProcessQueueAsync loop started. Waiting for initialization...");

            // Czekaj na sygnał z LoadHistory()
            await _initLatch.Task.ConfigureAwait(false);

            Debug.WriteLine("[DEBUG] Initialization complete. Queue ready to process messages.");

            try
            {
                // ConfigureAwait(false) zapobiega zamrażaniu UI i blokadom wątków
                await foreach (var (message, role) in _messageQueue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                {
                    Debug.WriteLine($"[DEBUG] Dequeued message for processing: {message}");
                    await ProcessMessageAsync(message, role).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[DEBUG] Queue processing task was canceled.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] Error in queue loop: {ex}");
            }
        }

        private async Task ProcessMessageAsync(string message, AuthorRole authorRole)
        {
            if (_chatSession == null)
            {
                Debug.WriteLine("[ERROR] ChatSession is NULL inside ProcessMessageAsync!");
                return;
            }

            try
            {
                _executorService.ResetSampling();
                var userMessage = new ChatHistory.Message(authorRole, message);

                Debug.WriteLine($"[DEBUG] Token generation starting for: {message}");

                await foreach (var token in _chatSession.ChatAsync(userMessage, _executorService.InferenceParams).ConfigureAwait(false))
                {
                    Debug.WriteLine($"TOKEN: '{token}'");
                    if (string.IsNullOrWhiteSpace(token)) continue;

                    _fullPrompt += token;
                    OnTokenGenerated?.Invoke(token);

                    if (_canceled)
                    {
                        OnTokenGenerated?.Invoke("[...stopped]");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LLama inference error: {ex}");
                OnTokenGenerated?.Invoke($"[error: {ex.Message}]");
            }
            finally
            {
                _canceled = false;
                _fullPrompt = "";
            }
        }

        public void Cancel() => _canceled = true;

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            try { _executorService.Dispose(); } catch { }
        }
    }
}