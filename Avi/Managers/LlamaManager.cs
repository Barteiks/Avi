using Avi.Services;
using Avi.Services.AI;
using LLama.Native;

namespace Avi.Managers
{
    public class LlamaManager : IDisposable
    {
        private LlamaSessionService? _session;

        public event Action<string>? OnTokenGenerated;

        public LlamaManager(ISpeechManager speechManager, ISettingsService settings)
        {
            NativeLibraryConfig.All.WithLogCallback((level, msg) =>
            {
                AppLogger.LogNative(level, msg);
            });
            var speechServer = speechManager.GetServer();
            _session = new LlamaSessionService(speechServer!, settings);
            _session.OnTokenGenerated += token =>
            {
                OnTokenGenerated?.Invoke(token);
            };
        }
        
        public async Task SendMessage(String message)
        {
            await _session!.SendMessageAsync(message);
        }
        public async Task LoadModel()
        {
            if (_session != null)
                await _session.Load(); // await the load so callers get accurate completion
        }

        public Task LoadHistory() => _session!.LoadHistory();

        public void Cancel() => _session?.Cancel();

        public void Dispose() => _session?.Dispose();
    }
}
