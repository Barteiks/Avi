using Avi.Managers;
using Avi.Services;
using Avi.Services.Llama;
using LLama.Native;
using System.Diagnostics;
using System.Text.Json;

public class TaskManager : IDisposable
{
    private readonly ISettingsService _settings;
    private LlamaManager? _ai;
    private ISpeechManager? _speech;
    private readonly AiResponseParser _parser = new();

    public event Action<string>? OnMessage;
    public event Action<string>? OnEmotion;
    public event Action<string>? OnStatus;
    public event Action<string>? OnDebug;

    public TaskManager(ISettingsService settings, IPlatformPathService platformPathService, ISpeechManager speechManager, LlamaManager llamaManager)
    {
        

        _settings = settings;

        _parser.OnParsedItem += HandleParsedItem;
        _speech = speechManager;
        _ai = llamaManager;

        // subscribe to speech debug/recognized notifications and forward to TaskManager.OnDebug (UI)
        _speech.OnRecognizedDebug += text =>
        {
            // ensure UI thread when MainPage is updated
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnDebug?.Invoke(text ?? string.Empty);
            });
        };
    }


    public async Task LoadAsync()
    {
#if ANDROID
        await Permissions.RequestAsync<Permissions.Microphone>();
#endif
        OnStatus?.Invoke("Loading models...");

        var whisperPath = _settings.WhisperPath;
        var llamaPath = _settings.LlamaPath;

        OnStatus?.Invoke("Loading Whisper...");
        await _speech!.LoadModel();

        OnStatus?.Invoke("Loading Llama...");
        await Task.Run(async () => await _ai!.LoadModel());

        OnStatus?.Invoke("Loading History...");
        await _ai!.LoadHistory();

        _ai.OnTokenGenerated += token =>
        {
            _parser.HandleToken(token);
        };

        OnStatus?.Invoke("Ready");
        _speech.Start();
        // send initial message (await so it's finished if you expect that)
        //await _ai!.SendMessage();
    }

    private void HandleParsedItem(JsonElement item)
    {
        // Defensive parsing: avoid exceptions from GetProperty when property is missing
        if (!item.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
            return;

        if (!item.TryGetProperty("content", out var contentProp))
            return;

        var type = typeProp.GetString();
        string content = contentProp.ValueKind == JsonValueKind.String
            ? contentProp.GetString() ?? string.Empty
            : contentProp.ToString() ?? string.Empty;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (type == "face")
                    OnEmotion?.Invoke(content);
                else if (type == "message")
                    OnMessage?.Invoke(content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskManager] HandleParsedItem error: {ex.Message}");
            }
        });
    }

    public void Dispose()
    {
        _ai.Dispose();
        _speech.Dispose();
    }
    public void Stop()
    {
        _ai.Cancel();
        _speech.Stop();

    }
    public void RaiseDebug(string message)
    {
        OnDebug?.Invoke(message ?? string.Empty);
    }
}