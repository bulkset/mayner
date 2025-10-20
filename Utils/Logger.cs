using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RvnMiner.Utils
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public class Logger
    {
        private static readonly object _lock = new object();
        private static Logger _instance;
        private readonly string _logFilePath;
        private readonly LogLevel _minLevel;
        private readonly bool _consoleOutput;
        private readonly long _maxFileSize;
        private readonly StreamWriter _writer;

        private Logger(string logFilePath, LogLevel minLevel, bool consoleOutput, long maxFileSizeMB)
        {
            _logFilePath = logFilePath;
            _minLevel = minLevel;
            _consoleOutput = consoleOutput;
            _maxFileSize = maxFileSizeMB * 1024 * 1024;

            // Создаем папку для логов если её нет
            var logDirectory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Проверяем размер файла и архивируем если нужно
            CheckLogFileSize();

            _writer = new StreamWriter(logFilePath, true, System.Text.Encoding.UTF8);
            _writer.AutoFlush = true;
        }

        public static Logger GetInstance(string logFilePath = "rvn-miner.log", LogLevel minLevel = LogLevel.Info, bool consoleOutput = true, long maxFileSizeMB = 10)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger(logFilePath, minLevel, consoleOutput, maxFileSizeMB);
                    }
                }
            }
            return _instance;
        }

        public void Log(LogLevel level, string message, Exception ex = null)
        {
            if (level < _minLevel) return;

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level.ToString(),
                Message = message,
                Exception = ex?.ToString()
            };

            string logText = JsonConvert.SerializeObject(logEntry);

            lock (_lock)
            {
                try
                {
                    _writer.WriteLine(logText);

                    if (_consoleOutput)
                    {
                        Console.ForegroundColor = GetConsoleColor(level);
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                        if (ex != null)
                        {
                            Console.WriteLine($"Exception: {ex.Message}");
                        }
                        Console.ResetColor();
                    }
                }
                catch (Exception e)
                {
                    // Если не можем записать в файл, пишем в консоль
                    Console.WriteLine($"Ошибка записи в лог: {e.Message}");
                    if (_consoleOutput)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                    }
                }
            }
        }

        private ConsoleColor GetConsoleColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message, Exception ex = null) => Log(LogLevel.Error, message, ex);
        public void Critical(string message, Exception ex = null) => Log(LogLevel.Critical, message, ex);

        private void CheckLogFileSize()
        {
            try
            {
                if (!File.Exists(_logFilePath)) return;

                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length > _maxFileSize)
                {
                    // Создаем архив старого лога
                    string archivePath = $"{_logFilePath}.{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                    File.Move(_logFilePath, archivePath);

                    // Очищаем старые архивы (оставляем только последние 5)
                    var logDirectory = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        var archiveFiles = Directory.GetFiles(logDirectory, "*.bak")
                            .OrderByDescending(f => f)
                            .Skip(5);

                        foreach (var oldArchive in archiveFiles)
                        {
                            try { File.Delete(oldArchive); } catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_consoleOutput)
                {
                    Console.WriteLine($"Ошибка при проверке размера лог-файла: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _writer?.Dispose();
            }
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }
            public string Exception { get; set; }
        }
    }
}