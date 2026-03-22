using Avi.Services.Whisper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Avi.Audio
{
    /// <summary>
    /// Orchestrator: wires platform recognizer and whisper transcription service to application listeners.
    /// This class is platform-agnostic.
    /// </summary>
    public class SpeechRecognitionServer : IDisposable
    {
        private readonly IPlatformSpeechRecognizer _recognizer;
        private readonly WhisperTranscriptionService _transcriptionService;

        // CHANGED: Add a paused flag here to allow callers to pause the transcription pipeline
        private volatile bool _isPaused;

        public readonly HashSet<Services.ISpeechListener> ServiceUsers = new();

        public SpeechRecognitionServer(IPlatformSpeechRecognizer recognizer, WhisperTranscriptionService transcriptionService)
        {
            _recognizer = recognizer;
            _transcriptionService = transcriptionService;

            _recognizer.SegmentReady += OnSegmentReady;
            _recognizer.Initialize();
        }

        public void Start()
        {
            _recognizer.Start();
            Debug.WriteLine("Speech recognition started.");
        }

        public void Stop()
        {
            _recognizer.Stop();
            Debug.WriteLine("Speech recognition stopped.");
        }

        private void OnSegmentReady(byte[] audioBytes)
        {
            // If paused, drop incoming segments quickly.
            if (_isPaused)
            {
                Debug.WriteLine("Segment received but transcription is paused; dropping.");
                return;
            }

            // Process asynchronously so the recognizer thread is not blocked.
            _ = Task.Run(async () =>
            {
                try
                {
                    var text = await _transcriptionService.TranscribeAsync(audioBytes);
                    if (string.IsNullOrWhiteSpace(text)) return;

                    Debug.WriteLine($"[DEBUG] Recognized text: {text}");

                    foreach (var listener in ServiceUsers.Where(x => x.IsInterested(text)))
                        listener.HandleSpeech(text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Transcription failed: {ex}");
                }
            });
        }

        public void Dispose()
        {
            _recognizer.SegmentReady -= OnSegmentReady;
            _recognizer.Dispose();
            _transcriptionService.Dispose();
        }
    }
}
