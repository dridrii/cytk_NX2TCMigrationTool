using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cytk_NX2TCMigrationTool.src.Core.Common.Utilities
{
    /// <summary>
    /// Logging levels from most to least verbose
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

    /// <summary>
    /// A thread-safe logger implementation that can write to file and console
    /// </summary>
    public class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();

        private readonly string _logFilePath;
        private readonly StreamWriter _logFileWriter;
        private LogLevel _logLevel;
        private bool _logToConsole;
        private readonly Queue<string> _pendingLogEntries = new Queue<string>();
        private readonly Task _logWriterTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Gets the singleton instance of the logger
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Current log level - messages below this level will not be logged
        /// </summary>
        public LogLevel LogLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        /// <summary>
        /// Whether to also log to the console
        /// </summary>
        public bool LogToConsole
        {
            get { return _logToConsole; }
            set { _logToConsole = value; }
        }

        /// <summary>
        /// Path to the current log file
        /// </summary>
        public string LogFilePath => _logFilePath;

        // Private constructor for singleton pattern
        private Logger()
        {
            // Default log level
            _logLevel = LogLevel.Info;
            _logToConsole = true;

            // Create log directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string logDir = Path.Combine(baseDir, "Logs");

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // Create log file with timestamp in name
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(logDir, $"nx2tc_log_{timestamp}.log");

            // Open log file for writing
            _logFileWriter = new StreamWriter(_logFilePath, true, Encoding.UTF8)
            {
                AutoFlush = true
            };

            // Write header to log file
            string header = $"=== NX2TC Migration Tool Log - Started at {DateTime.Now} ===";
            _logFileWriter.WriteLine(header);
            _logFileWriter.WriteLine(new string('=', header.Length));
            _logFileWriter.WriteLine();

            // Start the background log writer task
            _logWriterTask = Task.Run(() => LogWriterLoop(_cancellationTokenSource.Token));

            // Log that the logger has been initialized
            Log(LogLevel.Info, "Logger", "Logging system initialized");
        }

        /// <summary>
        /// Log a message with the specified level and source
        /// </summary>
        public void Log(LogLevel level, string source, string message)
        {
            if (level < _logLevel)
            {
                return; // Skip messages below the current log level
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{level}] [{source}] {message}";

            lock (_pendingLogEntries)
            {
                _pendingLogEntries.Enqueue(logEntry);
                Monitor.Pulse(_pendingLogEntries);
            }

            // Also log to console if enabled
            if (_logToConsole)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                // Set color based on log level
                switch (level)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Critical:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                }

                Console.WriteLine(logEntry);

                // Restore original color
                Console.ForegroundColor = originalColor;
            }
        }

        // Convenience methods for different log levels
        public void Trace(string source, string message) => Log(LogLevel.Trace, source, message);
        public void Debug(string source, string message) => Log(LogLevel.Debug, source, message);
        public void Info(string source, string message) => Log(LogLevel.Info, source, message);
        public void Warning(string source, string message) => Log(LogLevel.Warning, source, message);
        public void Error(string source, string message) => Log(LogLevel.Error, source, message);
        public void Critical(string source, string message) => Log(LogLevel.Critical, source, message);

        /// <summary>
        /// Background task to write log entries to file
        /// </summary>
        private void LogWriterLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string entry = null;

                    lock (_pendingLogEntries)
                    {
                        if (_pendingLogEntries.Count > 0)
                        {
                            entry = _pendingLogEntries.Dequeue();
                        }
                        else
                        {
                            // Wait for new log entries
                            Monitor.Wait(_pendingLogEntries);
                            continue;
                        }
                    }

                    if (entry != null)
                    {
                        // Write to log file
                        _logFileWriter.WriteLine(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                // If the logger itself fails, write directly to a fallback file
                try
                {
                    string fallbackPath = Path.Combine(
                        Path.GetDirectoryName(_logFilePath),
                        "logger_error.log");

                    File.AppendAllText(fallbackPath,
                        $"[{DateTime.Now}] Logger failure: {ex.Message}\r\n{ex.StackTrace}\r\n");
                }
                catch
                {
                    // Last resort - can't do much if this fails too
                }
            }
        }

        /// <summary>
        /// Clean up resources when the application is shutting down
        /// </summary>
        public void Shutdown()
        {
            // Log that we're shutting down
            Log(LogLevel.Info, "Logger", "Logging system shutting down");

            // Stop background task
            _cancellationTokenSource.Cancel();

            try
            {
                // Wait for any remaining log entries to be written
                _logWriterTask.Wait(1000);
            }
            catch { /* Ignore any exceptions during shutdown */ }

            // Close log file
            _logFileWriter.Flush();
            _logFileWriter.Close();
            _logFileWriter.Dispose();
        }
    }
}