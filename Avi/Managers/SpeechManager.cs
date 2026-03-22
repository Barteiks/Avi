using Avi.Audio;
using Avi.Services;
using Avi.Services.Whisper;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Avi.Managers
{
    public class SpeechManager : IDisposable, ISpeechManager
    {
        private SpeechRecognitionServer? _server;
        private WhisperTranscriptionService _whisper;

        public event Action<string?>? OnRecognizedDebug;

        public SpeechManager(ISettingsService settings)
        {
            // create whisper transcription service
            _whisper = new WhisperTranscriptionService(settings);

            // forward whisper debug/recognized messages to subscribers
            _whisper.OnRecognizedDebug += recognizedText =>
            {
                OnRecognizedDebug?.Invoke(recognizedText);
            };

            // create platform recognizer (add other platforms as needed)
            IPlatformSpeechRecognizer recognizer = null;
            
    #if ANDROID
                    recognizer = new Platforms.Android.SpeechRecognition();
    #elif WINDOWS
                    recognizer = new Platforms.Windows.SpeechRecognition();
    #endif
            // construct the orchestrator server
            _server = new SpeechRecognitionServer(recognizer, _whisper);
        }

        public async Task LoadModel()
        {
            await _whisper.CreateAsync();

        }

        public void Start() => _server?.Start();

        public void Stop() => _server?.Stop();

        public void Dispose()
        {
            _server?.Dispose();
            _server = null;
        }

        public SpeechRecognitionServer? GetServer() => _server;
    }
}
