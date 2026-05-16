using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Avi.Services.AI
{
    public class LlamaExecutorService : IDisposable
    {
        private LLamaWeights _model;
        private LLamaContext _context;
        public InteractiveExecutor _executor;
        private ModelParams _parameters;
        private ISettingsService _settings;
        public DefaultSamplingPipeline SamplingPipeline { get; set; }
        public InferenceParams InferenceParams { get; set; }
        private const uint MOBILE_CONTEXT_SIZE = 512;
        public LLamaContext Context => _context;
        public LLamaWeights Weights => _model;
        public ModelParams ModelParams => _parameters;
        
        public LlamaExecutorService(ISettingsService settings)
        {   
            _settings = settings;
        }
        private async Task<string> LoadGrammarAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("Assets/json.gbnf");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Trim();
        }
        public async Task LoadModel()
        {
            
            
            var gbnf = await LoadGrammarAsync();

            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = _settings.LlamaTemperature,
                Grammar = new Grammar(gbnf, "root"),
            };

            InferenceParams = new InferenceParams
            {
                SamplingPipeline = SamplingPipeline,
                MaxTokens = 500,
                AntiPrompts = new List<string> { "]" }
            };

            _parameters = new ModelParams(_settings.LlamaPath)
            {
                GpuLayerCount = _settings.LlamaGpuLayers,
                UseMemorymap = false,
                ContextSize = (uint?)_settings.LlamaContextSize,
                Threads = _settings.LlamaThreads,
                BatchSize = 64,
            };

            // Offload heavy native load to a background thread so UI isn't blocked
            try
            {
                await Task.Run(() =>
                {
                    _model = LLamaWeights.LoadFromFile(_parameters);
                    _context = _model.CreateContext(_parameters);
                    _executor = new InteractiveExecutor(_context);
                });
            }
            catch (Exception ex)
            {
                // rethrow to allow calling code to log / react
                throw new Exception("Failed to load LLama model: " + ex.Message, ex);
            }
        }

        public ChatSession CreateChatSession(ChatHistory history)
            => new ChatSession(_executor, history);

        public void ResetSampling() => SamplingPipeline.Reset();

        public void Dispose()
        {
            try { _model.Dispose(); } catch { }
            try { _context.Dispose(); } catch { }
        }
    }
}