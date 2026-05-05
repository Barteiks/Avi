using LLama;
using LLama.Common;
using LLama.Sampling;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avi.Services.AI
{
    public class ChatHistoryManager
    {
        private readonly string ChatHistoryFile = Path.Combine(AppDataDirectories.History, "chatHistory.json");
        private string _systemPrompt;

        public ChatHistory History { get; }
        public string SystemPrompt => _systemPrompt;

        public ChatHistoryManager()
        { 
            History = new ChatHistory();
        }
        public async Task InitializeAsync()
        {
            _systemPrompt = await ReadFileTEMP("Assets/promptMain.txt");
        }
        private async Task<string> ReadFileTEMP(String path)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Trim();
        }

        public void LoadHistory() { 
            // Wczytanie streszczeń poprzednich sesji
            if (File.Exists(ChatHistoryFile))
            {
                try
                {
                    var summaryJson = File.ReadAllText(ChatHistoryFile).Trim();
                    if (!string.IsNullOrEmpty(summaryJson))
                    {
                        var summaries = ChatHistory.FromJson(summaryJson);
                        if (summaries != null)
                        {
                            History.AddMessage(AuthorRole.Unknown, "The following is a summary of previous sessions. Use it only as background context.\n\n");
                            foreach (var msg in summaries.Messages)
                            {
                                // Dodajemy poprzednie streszczenia jako wiadomości typu Unknown
                                History.AddMessage(AuthorRole.Unknown,msg.Content);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load previous summaries: {ex.Message}");
                }
            }

            // Dodaj system prompt jako pierwszą wiadomość bieżącej sesji
            History.AddMessage(AuthorRole.System, _systemPrompt);
        }

        public void AddMessage(AuthorRole role, string content)
        {
            History.AddMessage(role, content);
        }

        // Generowanie streszczenia AI z ostatniej sesji
        public async Task GenerateAndSaveAISummaryAsync(LLamaWeights context, ModelParams tempParams, int maxTokens = 150)
        {
            var lastSessionPath = Path.Combine(AppDataDirectories.History, "lastSession.json");
            if (!File.Exists(lastSessionPath)) return;

            var lastSessionJson = File.ReadAllText(lastSessionPath).Trim();
            if (string.IsNullOrEmpty(lastSessionJson)) return;

            var lastSessionHistory = ChatHistory.FromJson(lastSessionJson);
            if (lastSessionHistory == null) return;

            // Filtrujemy wszystkie wiadomości System i scala w string
            var messagesToSummarize = lastSessionHistory.Messages
                .Where(m => m.AuthorRole != AuthorRole.System)
                .Where(m => m.AuthorRole != AuthorRole.Unknown)
                .Where(m => !m.Content.StartsWith("[Session Summary]")) // <- ignorujemy stare podsumowania
                .Select(m => $"{m.AuthorRole}: {m.Content}")
                .ToList();

            if (!messagesToSummarize.Any()) return;

            var summaryPrompt =
                "Summarize the conversation below into concise key facts and decisions.\n" +
                "Do NOT repeat dialogue verbatim. Do NOT continue conversation! Make it short. Do NOT quote! Text to summarize: \n\"" +
                string.Join("\n", messagesToSummarize) + "\"";

            // Tymczasowy pipeline bez Grammar
            var inferenceParams = new InferenceParams() { AntiPrompts = new List<string> {"\n\n"}, MaxTokens = 200 };


            var tempExecutor = new StatelessExecutor(context, tempParams);

            string summary = "";
            await foreach (var token in tempExecutor.InferAsync(summaryPrompt, inferenceParams))
            {
                summary += token;
            }

            summary = summary.Trim();
            if (string.IsNullOrWhiteSpace(summary)) return;

            // Zapis streszczenia do pliku
            

            ChatHistory summariesHistory;
            /*HEHEHEHEHEHEE
            if (File.Exists(ChatHistoryFile))
            {
                var existingJson = File.ReadAllText(ChatHistoryFile).Trim();
                summariesHistory = !string.IsNullOrEmpty(existingJson)
                    ? ChatHistory.FromJson(existingJson) ?? new ChatHistory()
                    : new ChatHistory();
            }
            else
            {
                summariesHistory = new ChatHistory();
            }
            
            summariesHistory.AddMessage(AuthorRole.Unknown, "[Session Summary]\n" + summary);
            File.WriteAllText(ChatHistoryFile, summariesHistory.ToJson());
            File.Delete(lastSessionPath);*/
        }
    }
}
