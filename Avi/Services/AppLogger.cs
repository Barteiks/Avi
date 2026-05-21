using LLama.Native;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Whisper.net.Logger;

namespace Avi.Services
{
    public static class AppLogger
    {
        private static string _logFilePath = null!;
        private static string _logPath = null!;
        public static ILogger Logger { get; set; }
        public static string _sessionLogFile;
        public static IPlatformPermissionManager _permissionManager;
        public static void Init(IPlatformPathService pathService, IPlatformPermissionManager permissionManager)
        {
            // Creating logs directory and log file
            var logsFolder = pathService.GetLogsDirectory();
            _sessionLogFile = Path.Combine(logsFolder, $"session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
            _logPath = logsFolder;
            _logFilePath = Path.Combine(logsFolder, "latest.log");
            _permissionManager = permissionManager;
            if (File.Exists(_logFilePath)) File.Delete(_logFilePath);
        }

        private static async Task WriteToFile(string level, string msg)
        {
            // Saving logs to file
            if (_logFilePath == null) return;
            var logLine = $"{DateTime.Now:HH:mm:ss} [{level}] {msg}";
            bool hasPermission = await _permissionManager.HasSpecialAsync(SpecialPermission.AllFilesAccess);
            if (hasPermission) { 
                try
                {
                    File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                    File.AppendAllText(_sessionLogFile, logLine + Environment.NewLine);
                }
                catch (Exception e) {
                    Logger?.LogWarning("Failed to save log: " + e);
                }
            }
            //Debug.WriteLine(logLine);
        }
        // Logging methods
        public static void Info(string msg)
        {
            Logger?.LogInformation(msg);
            WriteToFile("INFO", msg);
        }
        public static void Warning(string msg)
        {
            Logger?.LogWarning(msg);
            WriteToFile("Warning", msg);
        }
        public static void Error(string msg)
        {
            Logger?.LogError(msg);
            WriteToFile("ERROR", msg);
        }

        public static void LogNativeLlama(LLamaLogLevel level, string msg)
        {
            string formatted = $"[LlamaNative] {msg}";
            switch (level)
            {
                case LLamaLogLevel.Debug: goto default;
                case LLamaLogLevel.Warning: AppLogger.Warning(formatted); break;
                case LLamaLogLevel.Error: AppLogger.Error(formatted); break;
                case LLamaLogLevel.Continue: goto default;
                case LLamaLogLevel.None: goto default;
                default: AppLogger.Info(formatted); break;
            }
        }
        public static void LogNativeWhisper(WhisperLogLevel level, string msg)
        {
            string formatted = $"[WhisperNative] {msg}";
            switch (level)
            {
                case WhisperLogLevel.Debug: goto default;
                case WhisperLogLevel.Warning: AppLogger.Warning(formatted); break;
                case WhisperLogLevel.Error: AppLogger.Error(formatted); break;
                case WhisperLogLevel.Info: goto default;
                case WhisperLogLevel.None: goto default;
                default: AppLogger.Info(formatted); break;
            }
        }
    }
}