using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace Avi.Functions
{
    public class CmdFunction
    {
        private readonly TaskManager _taskManager;
        public event Action<string>? OnCmdCommandOutput;

        public CmdFunction(TaskManager taskManager)
        {
            _taskManager = taskManager;
            _taskManager.OnCmdCommand += async command =>
            {
                try
                {
                    // Pierwsze ostrzeżenie: Czy pozwolić na wykonanie?
#if WINDOWS
                    var result = MessageBox.Show(
                        $"Allow PowerShell command?\n\n{command}",
                        "Avi Security Warning",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.OK)
                        return;
#endif
                    // Uruchomienie komendy i odebranie czystego stringa
                    string output = await RunPowerShellAsync(command);

                    string message = $"[SYSTEM] Executed command: {command}, output: {output}";

                    // Drugie ostrzeżenie: Czy wysłać wynik do LLaMA?
#if WINDOWS
                    var result2 = MessageBox.Show(
                        $"Allow sending output to LLaMA?\n\n{message}",
                        "Avi Security Warning",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);

                    if (result2 != DialogResult.OK)
                        return;
#endif
                    OnCmdCommandOutput?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CmdFunction] Exception: {ex.Message}");
                }
            };
        }

        private async Task<string> RunPowerShellAsync(string command)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                // -NoProfile przyspiesza ładowanie, -NonInteractive blokuje wyskakiwanie promptów
                Arguments = $"-NoProfile -NonInteractive -Command \"{command.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = psi })
            {
                process.Start();

                // Czytamy strumienie równolegle, żeby proces się nie zawiesił przy dużym outputcie
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                // Czekamy na zakończenie procesu bez blokowania wątku UI
                await Task.Run(() => process.WaitForExit());

                string output = await outputTask;
                string error = await errorTask;

                // Jeśli komenda zwróciła błąd, doklejamy go do wyniku
                if (!string.IsNullOrWhiteSpace(error))
                {
                    return $"[ERROR] {error.Trim()} {output.Trim()}".Trim();
                }

                return output.Trim();
            }
        }
    }
}