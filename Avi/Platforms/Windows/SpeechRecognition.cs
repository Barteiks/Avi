using Avi.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Avi.Platforms.Windows
{
    internal class SpeechRecognition : IPlatformSpeechRecognizer, IDisposable
    {
        private WaveInEvent _waveIn;
        private readonly WaveFormat _waveFormat = new(16000, 16, 1);

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

        public event Action<byte[]> SegmentReady;

        public void Initialize()
        {
            _waveIn = new WaveInEvent
            {
                DeviceNumber = 0,
                BufferMilliseconds = FrameMs, // 🔥 ważne: stałe frame’y
                WaveFormat = _waveFormat
            };

            _waveIn.DataAvailable += OnAudioDataAvailable;
        }

        public void Start()
        {
            _waveIn?.StartRecording();
        }

        public void Stop()
        {
            _waveIn?.StopRecording();
        }

        private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
        {
            ProcessAudio(e.Buffer, e.BytesRecorded);
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

            // 🔥 smoothing
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
                    // 🔥 START
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
                _ = System.Threading.Tasks.Task.Run(() => handler.Invoke(audio));
        }

        public void Dispose()
        {
            if (_waveIn != null)
            {
                _waveIn.DataAvailable -= OnAudioDataAvailable;
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }
        }
    }
}