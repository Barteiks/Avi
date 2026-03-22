using Android.Media;
using Avi.Audio;
using System.Diagnostics;

namespace Avi.Platforms.Android
{
    internal class SpeechRecognition : IPlatformSpeechRecognizer, IDisposable
    {
        private AudioRecord? _audioRecord;
        private int _bufferSize;
        private CancellationTokenSource? _cts;

        private const int SampleRate = 16000;

        // 🔥 VAD CONFIG
        private const int FrameMs = 20;
        private const int PreBufferMs = 200;
        private const int SilenceMsToEnd = 600;
        private const float RmsThreshold = 500f;

        private readonly Queue<byte[]> _preBuffer = new();
        private readonly List<byte> _currentSegment = new();

        private bool _isSpeaking = false;
        private int _silenceDurationMs = 0;
        private float _smoothedRms = 0f;

        public event Action<byte[]>? SegmentReady;

        public void Initialize()
        {
            _bufferSize = AudioRecord.GetMinBufferSize(
                SampleRate,
                ChannelIn.Mono,
                Encoding.Pcm16bit);

            if (_bufferSize <= 0)
                _bufferSize = 4096;

            _bufferSize *= 2;

            _audioRecord = new AudioRecord(
                AudioSource.Mic,
                SampleRate,
                ChannelIn.Mono,
                Encoding.Pcm16bit,
                _bufferSize);

            if (_audioRecord.State != State.Initialized)
                throw new Exception("AudioRecord initialization failed");
        }

        public void Start()
        {
            if (_audioRecord == null)
                throw new InvalidOperationException("Initialize must be called first");

            if (_audioRecord.RecordingState == RecordState.Recording)
                return;

            _cts = new CancellationTokenSource();
            _audioRecord.StartRecording();

            Task.Run(() => ReadLoop(_cts.Token));
        }

        private async Task ReadLoop(CancellationToken token)
        {
            byte[] buffer = new byte[_bufferSize];

            while (!token.IsCancellationRequested)
            {
                var recorder = _audioRecord;
                if (recorder == null || recorder.RecordingState != RecordState.Recording)
                    break;

                int bytesRead = recorder.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    await Task.Delay(5);
                    continue;
                }

                ProcessAudio(buffer, bytesRead);
            }
        }

        private float CalculateRms(byte[] buffer, int bytesRead)
        {
            int samples = bytesRead / 2;
            double sum = 0;

            for (int i = 0; i < bytesRead; i += 2)
            {
                short sample = (short)((buffer[i + 1] << 8) | buffer[i]);
                sum += sample * sample;
            }

            return (float)Math.Sqrt(sum / samples);
        }

        private void ProcessAudio(byte[] buffer, int bytesRead)
        {
            float rms = CalculateRms(buffer, bytesRead);

            // 🔥 smoothing (bardzo ważne)
            _smoothedRms = 0.8f * _smoothedRms + 0.2f * rms;

            bool isSpeech = _smoothedRms > RmsThreshold;

            var frameCopy = buffer.Take(bytesRead).ToArray();

            // 🔹 PRE BUFFER
            _preBuffer.Enqueue(frameCopy);

            int maxPreFrames = PreBufferMs / FrameMs;
            while (_preBuffer.Count > maxPreFrames)
                _preBuffer.Dequeue();

            if (!_isSpeaking)
            {
                if (isSpeech)
                {
                    // 🔥 START MOWY
                    _isSpeaking = true;
                    _silenceDurationMs = 0;

                    _currentSegment.Clear();

                    foreach (var frame in _preBuffer)
                        _currentSegment.AddRange(frame);

                    Debug.WriteLine("VAD: START");
                }
            }
            else
            {
                _currentSegment.AddRange(frameCopy);

                if (isSpeech)
                {
                    _silenceDurationMs = 0;
                }
                else
                {
                    _silenceDurationMs += FrameMs;

                    if (_silenceDurationMs >= SilenceMsToEnd)
                    {
                        Debug.WriteLine("VAD: END");
                        FinishSegment();
                        _isSpeaking = false;
                    }
                }
            }
        }

        private void FinishSegment()
        {
            if (_currentSegment.Count == 0)
                return;

            var audio = _currentSegment.ToArray();

            _currentSegment.Clear();
            _preBuffer.Clear();

            var handler = SegmentReady;
            if (handler != null)
                Task.Run(() => handler.Invoke(audio));
        }

        public void Stop()
        {
            try
            {
                _cts?.Cancel();

                if (_audioRecord?.RecordingState == RecordState.Recording)
                    _audioRecord.Stop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();

                if (_audioRecord != null)
                {
                    _audioRecord.Release();
                    _audioRecord.Dispose();
                    _audioRecord = null;
                }

                _cts?.Dispose();
                _cts = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}