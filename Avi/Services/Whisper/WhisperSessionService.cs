using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net;

namespace Avi.Services.Whisper
{
    public class WhisperTranscriptionService : IDisposable
    {
        public event Action<string?>? OnRecognizedDebug;

        private WhisperProcessor? _processor;
        private readonly WaveFormat _waveFormat = new(16000, 16, 1);
        private readonly string[] _knownFalsePositives = new[] { "[BLANK_AUDIO]", "[silence]" };
        private readonly SemaphoreSlim _processingLock = new(1, 1);
        private readonly ISettingsService settings;

        public WhisperTranscriptionService(ISettingsService settings)
        {
            this.settings = settings;
        }

        public async Task CreateAsync()
        {
            var whisperFactory = WhisperFactory.FromPath(settings.WhisperPath);

            var build = whisperFactory.CreateBuilder()
                .WithThreads(4)
                .WithSingleSegment()
                .WithLanguage(settings.AppLanguage)
                .WithPrompt("User is speaking short real-time commands. Prefer clear words. If unclear, assume simple phrases.")
                .WithTemperature(0.0f);

            ((BeamSearchSamplingStrategyBuilder)build.WithBeamSearchSamplingStrategy())
                .WithPatience(1.0f)   // mniej zgadywania
                .WithBeamSize(3);

            _processor = build.Build();

            await Task.CompletedTask;
            Debug.WriteLine("Whisper initialized");
        }

        public async Task<string?> TranscribeAsync(byte[] audioBytes)
        {
            if (_processor == null)
                throw new InvalidOperationException("Whisper not initialized.");

            if (audioBytes == null || audioBytes.Length == 0)
                return null;

            double durationSec = (double)audioBytes.Length / _waveFormat.AverageBytesPerSecond;
            if (durationSec < 0.8)
                return null;

            await _processingLock.WaitAsync();

            try
            {
                await using var ms = new MemoryStream();

                WriteWavHeader(ms, audioBytes.Length, 16000, 1, 16);
                await ms.WriteAsync(audioBytes);
                ms.Seek(0, SeekOrigin.Begin);

                List<string> allTexts = new();

                await foreach (var segment in _processor.ProcessAsync(ms))
                {
                    if (!string.IsNullOrWhiteSpace(segment.Text))
                        allTexts.Add(segment.Text);
                }

                var text = string.Join(' ', allTexts).Trim();

                OnRecognizedDebug?.Invoke(text);

                // 🔴 3. Filtry jakości
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                if (text.Length < 3)
                    return null;

                if (text.All(c => !char.IsLetter(c)))
                    return null;

                if (_knownFalsePositives.Contains(text))
                    return null;

                return text;
            }
            finally
            {
                _processingLock.Release();
            }
        }

        // 🔥 VAD (RMS)
        private bool IsSpeech(byte[] audio)
        {
            short[] samples = new short[audio.Length / 2];
            Buffer.BlockCopy(audio, 0, samples, 0, audio.Length);

            double rms = Math.Sqrt(samples.Select(s => s * s).Average());

            Debug.WriteLine($"RMS: {rms}");

            return rms > 500; // możesz dostroić (300–1000)
        }

        private static void WriteWavHeader(Stream stream, int dataLength, int sampleRate, short channels, short bitsPerSample)
        {
            var writer = new BinaryWriter(stream);

            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataLength);
        }

        public void Dispose()
        {
            _processor?.Dispose();
            _processingLock?.Dispose();
        }
    }
}