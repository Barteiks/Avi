using Avi.Services;
using Avi.Services.AI;
using Avi.Services.Llama;
using LLama.Common;
using LLama.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Avi.Managers
{
    public class LlamaManager : IDisposable
    {
        
        private LlamaSessionService? _session;

        public event Action<string>? OnTokenGenerated;
        private ISettingsService _settings;

        public LlamaManager(ISpeechManager speechManager, ISettingsService settings)
        {
            _settings = settings;

            NativeLibraryConfig.All.WithLogCallback((level, msg) =>
            {
                AppLogger.LogNativeLlama(level, msg);
            });
            
            
           
            var speechServer = speechManager.GetServer();
            _session = new LlamaSessionService(speechServer!, settings);
            _session.OnTokenGenerated += token =>
            {
                OnTokenGenerated?.Invoke(token);
            };
        }
        
        public async Task SendMessage(String message, AuthorRole authorRole )
        {
            Debug.WriteLine("6767676767");
            await _session!.SendMessageAsync(message, authorRole);
        }
        public async Task LoadModel()
        {
            if (_settings.LlamaCUDA)
            {
                AppLogger.Info("Starting LLama for CUDA");
                NativeLibraryConfig.All.WithSelectingPolicy(new ForceCuda12Policy()).WithCuda(_settings.LlamaCUDA).WithAutoFallback(!_settings.LlamaCUDA).SkipCheck(_settings.LlamaCUDA);
            } 
            else
            {
                if (_settings.LlamaVulkan) AppLogger.Info("Starting LLama for Vulkan");
                NativeLibraryConfig.All.WithVulkan(_settings.LlamaVulkan);
            }
            if (_session != null)
                await _session.Load();
        }

        public Task LoadHistory() => _session!.LoadHistory();

        public void Cancel() => _session?.Cancel();

        public void Dispose() => _session?.Dispose();
    }
}
