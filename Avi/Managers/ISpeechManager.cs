using Avi.Audio;
using System;
using System.Threading.Tasks;

namespace Avi.Managers
{
    public interface ISpeechManager : IDisposable
    {
        Task LoadModel();
        void Start();
        void Stop();
        SpeechRecognitionServer? GetServer();

        // New event forwarded from the transcription service for debug/recognized text
        event Action<string?>? OnRecognizedDebug;
    }
}
