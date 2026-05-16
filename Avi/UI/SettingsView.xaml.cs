using Avi.Services;
using System;
using Microsoft.Maui.Controls;

namespace Avi.UI;

public partial class SettingsView : ContentView
{
    private readonly ISettingsService _settings;

    // Tablica dostępnych wartości kontekstu
    public int[] ContextValues { get; } = { 512, 1024, 2048, 4096, 8192 };

    // Maksymalna liczba wątków dostosowana do urządzenia
    public double MaxThreads => Environment.ProcessorCount;

    public SettingsView(ISettingsService settings)
    {
        InitializeComponent();
        _settings = settings;
        BindingContext = this;
    }

    // ==========================================
    // WHISPER SETTINGS
    // ==========================================

    public double WhisperContextIndex
    {
        get
        {
            int index = Array.IndexOf(ContextValues, _settings.WhisperContextSize);
            return index >= 0 ? index : 0; // Domyślnie ustawia 512
        }
        set
        {
            int rounded = (int)Math.Round(value);
            if (rounded >= 0 && rounded < ContextValues.Length && ContextValues[rounded] != _settings.WhisperContextSize)
            {
                _settings.WhisperContextSize = ContextValues[rounded]; // ZAPIS DO SERWISU
                OnPropertyChanged();
                OnPropertyChanged(nameof(WhisperContextSizeDisplay));
            }
        }
    }
    public int WhisperContextSizeDisplay => _settings.WhisperContextSize;

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
    public int LlamaContextSizeDisplay => _settings.LlamaContextSize;

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
}