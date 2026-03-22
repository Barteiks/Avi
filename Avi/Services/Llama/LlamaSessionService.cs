
using Avi.Audio;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLama.Transformers;
using System;
using System.Diagnostics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;


namespace Avi.Services.AI
{
    public class LlamaSessionService : ISpeechListener, IDisposable
    {
        private readonly LlamaExecutorService _executorService;
        private readonly ChatHistoryManager _historyManager;
        private SpeechRecognitionServer _speechServer;
        private readonly ISettingsService _settings;
        
        private ChatSession _chatSession;

        private bool _isResponding;
        private bool _canceled;
        private string _fullPrompt = "";

        // Event do MainWindow, żeby aktualizować textbox
        public event Action<string>? OnTokenGenerated;

        public LlamaSessionService(SpeechRecognitionServer speechServer, ISettingsService settings)
        {
            _executorService = new LlamaExecutorService(settings);
            _historyManager = new ChatHistoryManager();
            _speechServer = speechServer;
            _speechServer.ServiceUsers.Add(this);
            _settings = settings; // CHANGED: assign
        }
        public async Task Load()
        {
            //executor
            await _executorService.LoadModel();
            //speech

        }

        public bool IsInterested(string audioTranscription)
            => !_isResponding || audioTranscription.Contains("stop", StringComparison.CurrentCultureIgnoreCase);

        public void HandleSpeech(string audioTranscription)
        {
            if (_isResponding && audioTranscription.Contains("stop", StringComparison.CurrentCultureIgnoreCase))
            {
                _canceled = true;
            }
            else if (!_isResponding)
            {
                _ = SendMessageAsync(audioTranscription);
            }
        }
        public async Task LoadHistory()
        {
            await _historyManager.GenerateAndSaveAISummaryAsync(_executorService.Weights, _executorService.ModelParams);
            _historyManager.LoadHistory();
            _chatSession = _executorService.CreateChatSession(_historyManager.History);
            string historyString = _historyManager.History.ToJson();
            Debug.Print(historyString);
        }
        public async Task OnSpeechRecognizedAsync(string text)
        {
            Debug.WriteLine($"[DEBUG] Received text for AI: {text}");
            await SendMessageAsync(text);
        }

        public async Task SendMessageAsync(string message)
        {
            _isResponding = true;

            try
            {
                _executorService.ResetSampling();

                var userMessage = new ChatHistory.Message(AuthorRole.User, message);

                var prompt =
                $"""
                {_historyManager.SystemPrompt}

                User question:
                {message?.Trim()}

                JSON response:
                """;
                if (_chatSession == null || _executorService == null)
                {
                    Debug.WriteLine("[ERROR] ChatSession is NULL");
                    return;
                }
                
                try
                {
                    Debug.WriteLine("ChatAsync iteration...");
                    
                        Debug.WriteLine("ChatAsync iteration...2");
                    Debug.WriteLine(message);
                    await foreach (var token in _chatSession.ChatAsync(userMessage, _executorService.InferenceParams))
                        
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
                    _isResponding = false;
                    _canceled = false;
                    //var historyFile = Path.Combine(AppDataDirectories.History, "lastSession.json");
                    //File.WriteAllText(historyFile, _historyManager.History.ToJson());
                    _fullPrompt = "";
                }
            }
            finally
            {
                
            }
        }

        private void AddToPrompt(string msg) => _fullPrompt += msg;

        public void Cancel() => _canceled = true;

        public void Dispose()
        {
            try { _executorService.Dispose(); } catch { }
        }
    }
}

