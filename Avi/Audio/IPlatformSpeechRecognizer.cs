
using System;

namespace Avi.Audio
{
    /// <summary>
    /// Platform-agnostic recognizer interface. Implementations capture audio,
    /// detect speech segments and raise them as byte[] (raw PCM WAV payload).
    /// </summary>
    public interface IPlatformSpeechRecognizer : IDisposable
    {
        /// <summary>
        /// Raised when a speech segment is ready to transcribe.
        /// The handler should be non-blocking; consumers may call async methods.
        /// </summary>
        event Action<byte[]> SegmentReady;

        /// <summary>
        /// Initialize platform resources (set up device, buffers, etc.).
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start capturing audio.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop capturing audio.
        /// </summary>
        void Stop();
    }
}