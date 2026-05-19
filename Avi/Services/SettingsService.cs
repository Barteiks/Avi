using System;
using System.IO;
using Microsoft.Maui.Storage;

namespace Avi.Services
{
    public interface ISettingsService
    {
        // Model paths
        string WhisperPath { get; set; }
        string LlamaPath { get; set; }

        // LLama _settings
        int LlamaGpuLayers { get; set; }
        int LlamaContextSize { get; set; }
        int LlamaThreads { get; set; }
        float LlamaTemperature { get; set; }
        bool LlamaCUDA { get; set; }
        bool LlamaVulkan { get; set; }

        // Whisper _settings
        int WhisperContextSize { get; set; }
        int WhisperThreads { get; set; }
        bool WhisperCUDA { get; set; }
        bool WhisperVulkan { get; set; }

        // App _settings
        string AppLanguage { get; set; }
        bool IsFirstLaunch { get; set; }

        void ClearAll();
    }

    public class SettingsService : ISettingsService
    {
        private readonly IPlatformPathService _platformPathService;

        private static class Keys
        {
            // App _settings
            public const string AppLanguage = "app_language";
            public const string FirstLaunch = "first_launch";

            // Whisper _settings
            public const string WhisperModelPath = "whisper_file";
            public const string WhisperThreads = "whisper_threads";
            public const string WhisperContextSize = "whisper_context_size";
            public const string WhisperCUDA = "whisper_cuda";
            public const string WhisperVulkan = "whisper_vulkan";

            // Llama _settings
            public const string LlamaModelPath = "llama_file";
            public const string LlamaThreads = "llama_threads";
            public const string LlamaGpuLayers = "llama_gpu_layers";
            public const string LlamaContextSize = "llama_context_size";
            public const string LlamaTemperature = "llama_temperature";
            public const string LlamaCUDA = "llama_cuda";
            public const string LlamaVulkan = "llama_vulkan";
        }

        public SettingsService(IPlatformPathService platformPathService)
        {
            _platformPathService = platformPathService;
        }

        // -------------------------
        // MODEL PATHS
        // -------------------------

        public string WhisperPath
        {
            get
            {
                var folder = _platformPathService.GetModelDirectory();
                var fileName = Preferences.Get(Keys.WhisperModelPath, "ggml-tiny-q5_1.bin"); //ggml-medium-q5_0.bin
                return Path.Combine(folder, fileName);
            }
            set
            {
                var fileName = Path.GetFileName(value);
                Preferences.Set(Keys.WhisperModelPath, fileName);
            }
        }

        public string LlamaPath
        {
            get
            {
                var folder = _platformPathService.GetModelDirectory();
                var fileName = Preferences.Get(Keys.LlamaModelPath, "xd.gguf"); //Meta-Llama-3.1-8B-Instruct-Q5_K_M
                return Path.Combine(folder, fileName);
            }
            set
            {
                var fileName = Path.GetFileName(value);
                Preferences.Set(Keys.LlamaModelPath, fileName);
            }
        }

        // -------------------------
        // LLAMA SETTINGS
        // -------------------------

        public int LlamaGpuLayers
        {
            get => Preferences.Get(Keys.LlamaGpuLayers, 200);
            set => Preferences.Set(Keys.LlamaGpuLayers, value);
        }

        public int LlamaContextSize
        {
            get => Preferences.Get(Keys.LlamaContextSize, 1024); // default 4 GB
            set => Preferences.Set(Keys.LlamaContextSize, value);
        }

        public int LlamaThreads
        {
            get => Preferences.Get(Keys.LlamaThreads, Environment.ProcessorCount - 1);//);
            set => Preferences.Set(Keys.LlamaThreads, value);
        }

        public float LlamaTemperature
        {
            get => Preferences.Get(Keys.LlamaTemperature, 0.7f);
            set => Preferences.Set(Keys.LlamaTemperature, value);
        }

        public bool LlamaCUDA
        {
            get => Preferences.Get(Keys.LlamaCUDA, true);
            set => Preferences.Set(Keys.LlamaCUDA, value);
        }
        public bool LlamaVulkan
        {
            get => Preferences.Get(Keys.LlamaVulkan, true);
            set => Preferences.Set(Keys.LlamaVulkan, value);
        }

        // -------------------------
        // WHISPER SETTINGS
        // -------------------------

        public int WhisperContextSize
        {
            get => Preferences.Get(Keys.WhisperContextSize, 512);
            set => Preferences.Set(Keys.WhisperContextSize, value);
        }

        public int WhisperThreads
        {
            get => Preferences.Get(Keys.WhisperThreads, Environment.ProcessorCount);
            set => Preferences.Set(Keys.WhisperThreads, value);
        }

        public bool WhisperCUDA
        {
            get => Preferences.Get(Keys.WhisperCUDA, true);
            set => Preferences.Set(Keys.WhisperCUDA, value);
        }
        public bool WhisperVulkan
        {
            get => Preferences.Get(Keys.WhisperVulkan, true);
            set => Preferences.Set(Keys.WhisperVulkan, value);
        }

        // -------------------------
        // APP SETTINGS
        // -------------------------

        public string AppLanguage
        {
            get => Preferences.Get(Keys.AppLanguage, "en");
            set => Preferences.Set(Keys.AppLanguage, value);
        }

        public bool IsFirstLaunch
        {
            get => Preferences.Get(Keys.FirstLaunch, true);
            set => Preferences.Set(Keys.FirstLaunch, value);
        }

        // -------------------------
        // CLEAR ALL SETTINGS
        // -------------------------

        public void ClearAll()
        {
            Preferences.Clear();
        }
    }
}