using Avi.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace Avi.UI;

public partial class SettingsView : ContentView
{
    private readonly ISettingsService _settings;
    private readonly IPlatformPermissionManager _permissionManager;
    private readonly IPlatformPathService _pathService;
    public ObservableCollection<string> WhisperModels { get; set; } = new();
    public ObservableCollection<string> LlamaModels { get; set; } = new();
    // Tablica dostępnych wartości kontekstu
    public int[] ContextValues { get; } = { 512, 1024, 2048, 4096, 8192, 10240, 12288, 16384, 24576, 32768};
    public int[] GpuLayerValues { get; } =
    {
        -1,10,20,30, 40,60,80,120, 160, 200, 250, 300, 500
    };
    public int[] BatchSizeValues { get; } =
    {
        64,
        128,
        256,
        512,
        1024,
        2048
    };
    // Maksymalna liczba wątków dostosowana do urządzenia
    public double MaxThreads => Environment.ProcessorCount;

    public SettingsView(ISettingsService settings, IPlatformPermissionManager permissionManager, IPlatformPathService pathService)
    {
        InitializeComponent();
        _settings = settings;
        _permissionManager = permissionManager;
        _pathService = pathService;
        BindingContext = this;
        LoadModels(Model.Whisper);
        LoadModels(Model.Llama);
    }
    public void Refresh()
    {
        LoadModels(Model.Whisper);
        LoadModels(Model.Llama);
    }
    
    public string SelectedModelWhisper
    {
        get {
            if (WhisperModels.Contains(_settings.WhisperModel))
            {
                return _settings.WhisperModel;
            }
            else
            {
                return "Import";
            }
        }
        set
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (value == "Import")
            {
                return;
            }
            if (_settings.WhisperModel != value)
            {
                _settings.WhisperModel = value;
                OnPropertyChanged();
            }
        }
    }
    public string SelectedModelLlama
    {
        get
        {
            if (LlamaModels.Contains(_settings.LlamaModel))
            {
                return _settings.LlamaModel;
            }
            else
            {
                return "Import";
            }
        }
        set
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (value == "Import")
            {
                return;
            }
            if (_settings.LlamaModel != value)
            {
                _settings.LlamaModel = value;
                OnPropertyChanged();
            }
        }
    }
    public enum Model
    {
        Whisper,
        Llama
    }

    public async Task LoadModels(Model model)
    {
        bool hasStorageAcces = await _permissionManager.HasSpecialAsync(SpecialPermission.AllFilesAccess);
        if (!hasStorageAcces)
        {
            return;
        }
        string format = "*.bin";
        if (model == Model.Whisper)
        {
            WhisperModelPicker.IsEnabled = true;
        }
        else
        {
            LlamaModelPicker.IsEnabled = true;
            format= "*.gguf";
        }
        var folderPath = _pathService.GetModelDirectory();
        if (!Directory.Exists(folderPath)) return;

        var binFiles = Directory.GetFiles(folderPath, format)
                                .Select(Path.GetFileName)
                                .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (model == Model.Whisper)
            {
                WhisperModels.Clear();
                foreach (var file in binFiles)
                {
                    WhisperModels.Add(file);
                }
                WhisperModels.Add("Import");
                OnPropertyChanged(nameof(SelectedModelWhisper));
            }
            else if (model == Model.Llama)
            {
                LlamaModels.Clear();
                foreach (var file in binFiles)
                {
                    LlamaModels.Add(file);
                }
                LlamaModels.Add("Import");
                OnPropertyChanged(nameof(SelectedModelLlama));

            }
        });
    }

    // ==========================================
    // WHISPER SETTINGS
    // ==========================================

    public double WhisperTemperatureIndex
    {
        get => _settings.WhisperTemperature * 10.0;
        set
        {
            double snapped = Math.Round(value);
            float temp = (float)(snapped / 10.0);

            if (Math.Abs(_settings.WhisperTemperature - temp) > 0.001f)
            {
                _settings.WhisperTemperature = temp;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WhisperTemperatureDisplay));
            }
        }
    }
    public float WhisperTemperatureDisplay => _settings.WhisperTemperature;


    public double WhisperThreads
    {
        get => _settings.WhisperThreads;
        set
        {
            int rounded = (int)Math.Round(value);
            if (_settings.WhisperThreads != rounded)
            {
                _settings.WhisperThreads = rounded; // ZAPIS DO SERWISU
                OnPropertyChanged();
            }
        }
    }

    public bool WhisperCUDA
    {
        get => _settings.WhisperCUDA;
        set
        {
            if (_settings.WhisperCUDA != value)
            {
                _settings.WhisperCUDA = value; // ZAPIS DO SERWISU
                OnPropertyChanged();
            }
        }
    }
    public bool WhisperVulkan
    {
        get => _settings.WhisperVulkan;
        set
        {
            if (_settings.WhisperVulkan != value)
            {
                _settings.WhisperVulkan = value; // ZAPIS DO SERWISU
                OnPropertyChanged();
            }
        }
    }

    // ==========================================
    // LLAMA SETTINGS
    // ==========================================

    public double LlamaContextIndex
    {
        get
        {
            int index = Array.IndexOf(ContextValues, _settings.LlamaContextSize);
            return index >= 0 ? index : 1; // Domyślnie ustawia 1024
        }
        set
        {
            int rounded = (int)Math.Round(value);
            if (rounded >= 0 && rounded < ContextValues.Length && ContextValues[rounded] != _settings.LlamaContextSize)
            {
                _settings.LlamaContextSize = ContextValues[rounded]; // ZAPIS DO SERWISU
                OnPropertyChanged();
                OnPropertyChanged(nameof(LlamaContextSizeDisplay));
            }
        }
    }
    public double LlamaGpuLayersIndex
    {
        get
        {
            int index = Array.IndexOf(GpuLayerValues, _settings.LlamaGpuLayers);
            return index >= 0 ? index : 1;
        }
        set
        {
            int rounded = (int)Math.Round(value);
            if (rounded >= 0 && rounded < GpuLayerValues.Length && GpuLayerValues[rounded] != _settings.LlamaGpuLayers)
            {
                _settings.LlamaGpuLayers = GpuLayerValues[rounded]; // ZAPIS DO SERWISU
                OnPropertyChanged();
                OnPropertyChanged(nameof(LlamaGpuLayersDisplay));
            }
        }
    }
    public double LlamaBatchSizeIndex
    {
        get
        {
            int index = Array.IndexOf(BatchSizeValues, _settings.LlamaBatchSize);
            return index >= 0 ? index : 1;
        }
        set
        {
            int rounded = (int)Math.Round(value);
            if (rounded >= 0 && rounded < BatchSizeValues.Length && BatchSizeValues[rounded] != _settings.LlamaBatchSize)
            {
                _settings.LlamaBatchSize = BatchSizeValues[rounded]; // ZAPIS DO SERWISU
                OnPropertyChanged();
                OnPropertyChanged(nameof(LlamaBatchSizeDisplay));
            }
        }
    }
    public double LlamaTemperatureIndex
    {
        get => _settings.LlamaTemperature * 10.0;
        set
        {
            double snapped = Math.Round(value);
            float temp = (float)(snapped / 10.0);

            if (Math.Abs(_settings.LlamaTemperature - temp) > 0.001f)
            {
                _settings.LlamaTemperature = temp;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LlamaTemperatureDisplay));
            }
        }
    }
    public float LlamaTemperatureDisplay => _settings.LlamaTemperature;
    public int LlamaBatchSizeDisplay => _settings.LlamaBatchSize;
    public int LlamaContextSizeDisplay => _settings.LlamaContextSize;
    public string LlamaGpuLayersDisplay =>
    _settings.LlamaGpuLayers == -1
        ? "AUTO"
        : _settings.LlamaGpuLayers.ToString();

    public double LlamaThreads
    {
        get => _settings.LlamaThreads;
        set
        {
            int rounded = (int)Math.Round(value);
            if (_settings.LlamaThreads != rounded)
            {
                _settings.LlamaThreads = rounded; // ZAPIS DO SERWISU
                OnPropertyChanged();
            }
        }
    }

    public bool LlamaCUDA
    {
        get => _settings.LlamaCUDA;
        set
        {
            if (_settings.LlamaCUDA != value)
            {
                _settings.LlamaCUDA = value; // ZAPIS DO SERWISU
                OnPropertyChanged();
            }
        }
    }
    public bool LlamaVulkan
    {
        get => _settings.LlamaVulkan;
        set
        {
            if (_settings.LlamaVulkan != value)
            {
                _settings.LlamaVulkan = value;
                OnPropertyChanged();
            }
        }
    }
}