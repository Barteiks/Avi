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
        string LlamaModel { get; set; }
        string LlamaLanguage { get; set; }
        int LlamaGpuLayers { get; set; }
        int LlamaContextSize { get; set; }
        int LlamaMaxTokens { get; set; }
        int LlamaThreads { get; set; }
        int LlamaBatchSize { get; set; }
        float LlamaTemperature { get; set; }
        bool LlamaCUDA { get; set; }
        bool LlamaVulkan { get; set; }

        // Whisper _settings
        string WhisperModel { get; set; }
        string WhisperLanguage { get; set; }
        int WhisperThreads { get; set; }
        float WhisperTemperature { get; set; }
        bool WhisperCUDA { get; set; }
        bool WhisperVulkan { get; set; }

        // App _settings
        bool IsFirstLaunch { get; set; }

        void ClearAll();
    }

    public class SettingsService : ISettingsService
    {
        private readonly IPlatformPathService _platformPathService;

        private static class Keys
        {
            // App _settings
            public const string FirstLaunch = "first_launch";

            // Whisper _settings
            public const string WhisperModel = "whisper_model";
            public const string WhisperLanguage = "whisper_language";
            public const string WhisperModelPath = "whisper_file";
            public const string WhisperThreads = "whisper_threads";
            public const string WhisperTemperature = "whisper_temperature";
            public const string WhisperCUDA = "whisper_cuda";
            public const string WhisperVulkan = "whisper_vulkan";

            // Llama _settings
            public const string LlamaModel = "llama_model";
            public const string LlamaLanguage = "llama_language";
            public const string LlamaModelPath = "llama_file";
            public const string LlamaThreads = "llama_threads";
            public const string LlamaGpuLayers = "llama_gpu_layers";
            public const string LlamaContextSize = "llama_context_size";
            public const string LlamaMaxTokens = "llama_max_tokens";
            public const string LlamaTemperature = "llama_temperature";
            public const string LlamaBatchSize = "llama_batch_size";
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
        public string WhisperModel
        {
            get => Preferences.Get(Keys.WhisperModel, "");
            set => Preferences.Set(Keys.WhisperModel, value);
        }

        public string WhisperPath
        {
            get
            {
                var folder = _platformPathService.GetModelDirectory();
                var fileName = WhisperModel;
                return Path.Combine(folder, fileName);
            }
            set
            {
                var fileName = Path.GetFileName(value);
                Preferences.Set(Keys.WhisperModelPath, fileName);
            }
        }
        public string LlamaModel
        {
            get => Preferences.Get(Keys.LlamaModel, "");
            set => Preferences.Set(Keys.LlamaModel, value);
        }

        public string LlamaPath
        {
            get
            {
                var folder = _platformPathService.GetModelDirectory();
                var fileName = LlamaModel;
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
        public string LlamaLanguage
        {
            get => Preferences.Get(Keys.LlamaLanguage, "en");
            set => Preferences.Set(Keys.LlamaLanguage, value);
        }
        public int LlamaGpuLayers
        {
            get => Preferences.Get(Keys.LlamaGpuLayers, -1);
            set => Preferences.Set(Keys.LlamaGpuLayers, value);
        }

        public int LlamaContextSize
        {
            get => Preferences.Get(Keys.LlamaContextSize, 4096); // default 4 GB
            set => Preferences.Set(Keys.LlamaContextSize, value);
        }

        public int LlamaThreads
        {
            get => Preferences.Get(Keys.LlamaThreads, Environment.ProcessorCount - 1);//);
            set => Preferences.Set(Keys.LlamaThreads, value);
        }

        public int LlamaMaxTokens
        {
            get => Preferences.Get(Keys.LlamaMaxTokens, 500);
            set => Preferences.Set(Keys.LlamaMaxTokens, value);
        }

        public float LlamaTemperature
        {
            get => Preferences.Get(Keys.LlamaTemperature, 0.7f);
            set => Preferences.Set(Keys.LlamaTemperature, value);
        }
        
        public int LlamaBatchSize
        {
            get => Preferences.Get(Keys.LlamaBatchSize, 64);
            set => Preferences.Set(Keys.LlamaBatchSize, value);
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
        public string WhisperLanguage
        {
            get => Preferences.Get(Keys.WhisperLanguage, "en");
            set => Preferences.Set(Keys.WhisperLanguage, value);
        }

        public int WhisperThreads
        {
            get => Preferences.Get(Keys.WhisperThreads, Environment.ProcessorCount - 1);
            set => Preferences.Set(Keys.WhisperThreads, value);
        }
        public float WhisperTemperature
        {
            get => Preferences.Get(Keys.WhisperTemperature, 0.0f);
            set => Preferences.Set(Keys.WhisperTemperature, value);
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