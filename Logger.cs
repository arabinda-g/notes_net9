using System;
using System.IO;
using System.Text;

namespace Notes
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    public static class Logger
    {
        private static LogLevel _currentLogLevel = LogLevel.None;
        private static string _logFilePath = string.Empty;
        private static readonly object _lockObj = new object();
        private static bool _initialized = false;

        public static LogLevel CurrentLogLevel
        {
            get => _currentLogLevel;
            set => _currentLogLevel = value;
        }

        public static void Initialize(LogLevel logLevel = LogLevel.None)
        {
            if (_initialized && _currentLogLevel == logLevel)
                return;

            _currentLogLevel = logLevel;

            if (_currentLogLevel != LogLevel.None)
            {
                try
                {
                    // Get the directory where the exe is located
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string exeDir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                    
                    string fileName = $"Notes_{DateTime.Now:yyyyMMdd}.log";
                    _logFilePath = Path.Combine(exeDir, fileName);
                    
                    _initialized = true;
                    
                    // Write initialization message
                    Log(LogLevel.Info, $"Logging initialized - Level: {logLevel}");
                    Log(LogLevel.Info, $"Log file: {_logFilePath}");
                    Log(LogLevel.Info, new string('=', 60));
                }
                catch (Exception ex)
                {
                    // Silently fail if we can't initialize logging
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize logger: {ex.Message}");
                }
            }
            else
            {
                _initialized = true;
            }
        }

        public static void Log(LogLevel level, string message)
        {
            if (_currentLogLevel == LogLevel.None || level > _currentLogLevel)
                return;

            if (string.IsNullOrEmpty(_logFilePath))
                Initialize(_currentLogLevel);

            try
            {
                lock (_lockObj)
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string levelStr = level.ToString().ToUpper().PadRight(7);
                    string logEntry = $"[{timestamp}] [{levelStr}] {message}";

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
                    
                    // Also output to debug console
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
            catch (Exception)
            {
                // Silently fail if we can't write to log
            }
        }

        public static void Error(string message) => Log(LogLevel.Error, message);
        public static void Warning(string message) => Log(LogLevel.Warning, message);
        public static void Info(string message) => Log(LogLevel.Info, message);
        public static void Debug(string message) => Log(LogLevel.Debug, message);

        public static void Error(string message, Exception ex) 
            => Log(LogLevel.Error, $"{message} - Exception: {ex.Message}\nStack: {ex.StackTrace}");

        public static void LogMethodEntry(string methodName)
            => Debug($">> Entering: {methodName}");

        public static void LogMethodExit(string methodName)
            => Debug($"<< Exiting: {methodName}");

        public static string GetLogFilePath() => _logFilePath;

        public static void ClearOldLogs(int daysToKeep = 7)
        {
            try
            {
                if (string.IsNullOrEmpty(_logFilePath))
                    return;

                string logDir = Path.GetDirectoryName(_logFilePath);
                if (string.IsNullOrEmpty(logDir) || !Directory.Exists(logDir))
                    return;

                var files = Directory.GetFiles(logDir, "Notes_*.log");
                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    try
                    {
                        if (File.GetCreationTime(file) < cutoffDate)
                        {
                            File.Delete(file);
                            Debug($"Deleted old log file: {Path.GetFileName(file)}");
                        }
                    }
                    catch
                    {
                        // Skip files we can't delete
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear old logs: {ex.Message}");
            }
        }
    }
}

