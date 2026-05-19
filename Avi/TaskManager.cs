using Avi.Functions;
using Avi.Managers;
using Avi.Services;
using Avi.Services.Llama;
using LLama.Native;
using System.Diagnostics;
using System.Text.Json;

public class TaskManager : IDisposable
{
    private readonly ISettingsService _settings;
    private readonly IPlatformPermissionManager _permissionManager;
    private LlamaManager? _ai;
    private ISpeechManager? _speech;
    private readonly AiResponseParser _parser = new();
    private bool _isLoaded = false;
    public event Action<string>? OnMessage;
    public event Action<string>? OnEmotion;
    public event Action<string>? OnStatus;
    public event Action<string>? OnDebug;
    public event Action<string>? OnCmdCommand;
    private FunctionsManager _functions;
    public TaskManager(ISettingsService settings, IPlatformPathService platformPathService, ISpeechManager speechManager, LlamaManager llamaManager, IPlatformPermissionManager permissionManager)
    {
        _settings = settings;
        _permissionManager = permissionManager;
        _parser.OnParsedItem += HandleParsedItem;
        _speech = speechManager;
        _ai = llamaManager;
        _functions = new FunctionsManager(this);
        _functions.loadFunctions();
        // subscribe to speech debug/recognized notifications and forward to TaskManager.OnDebug (UI)
        _speech.OnRecognizedDebug += text =>
        {
            // ensure UI thread when MainPage is updated
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnDebug?.Invoke(text ?? string.Empty);
            });
        };
        _functions.OnCmdCommandOutput += async message =>
        {
            Debug.WriteLine($"[CmdFunction] Received command AMOGUS: {message}");
            await llamaManager.SendMessage(message, LLama.Common.AuthorRole.User);
        };
    }


    public async Task LoadAsync()
    {
        _isLoaded = true;
        try
        {
            var microphonePermission = await _permissionManager.RequestAsync<Permissions.Microphone>();
            if (microphonePermission != PermissionStatus.Granted)
            {
                OnStatus?.Invoke("Microphone permission denied. Please grant permissions and restart the app.");
                _isLoaded = false;
                return;
            }
        }
        catch (Exception ex) { 
         AppLogger.Error($"Error requesting microphone permission: {ex.Message}");
        }
#if ANDROID
        await _permissionManager.RequestSpecialAsync(SpecialPermission.AllFilesAccess);
        if (! await _permissionManager.HasSpecialAsync(SpecialPermission.AllFilesAccess))
        {
            OnStatus?.Invoke("No permissions!!!...");
            _isLoaded = false;
            return;
        }
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
        if (type == "cmd")
        {
            // Handle command if needed
            Debug.WriteLine($"[TaskManager] Received command: {content}");
            OnCmdCommand?.Invoke(content);

        }
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
    public bool isLoaded => _isLoaded;
}