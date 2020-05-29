using System;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using Serilog;
using Serilog.Core;
using VnManager.Converters;

namespace VnManager.Utilities
{
    internal static class LogManager
    {
        public static LogLevel LogLevel { get; private set; } = LogLevel.Normal;

        private static ILogger _logger;
        public static ILogger Logger
        {
            get { return _logger ??= SetInitialLogger(); }
            private set => _logger = value;
        }
        
        private static string GetConfigDirectory()
        {
            return string.IsNullOrEmpty(App.ConfigDirPath) ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) : App.ConfigDirPath;
        }

        private static ILogger SetInitialLogger()
        {
            return new LoggerConfiguration().WriteTo.File(new SerilogFormatter(), $@"{GetConfigDirectory()}\logs\{DateTime.Now:dd-MM-yyyy}_{LogLevel.ToString()}.log",
                fileSizeLimitBytes: 500000, rollOnFileSizeLimit: true, retainedFileCountLimit: 15).CreateLogger();
        }

        internal static void UpdateLoggerDirectory()
        {
            if (!App.StartupLockout)//disallow updating logger after App has started
            {
                var logConfig = new LoggerConfiguration().WriteTo.File(new SerilogFormatter(), $@"{GetConfigDirectory()}\logs\{DateTime.Now:dd-MM-yyyy}_{LogLevel.ToString()}.log",
                    fileSizeLimitBytes: 500000, rollOnFileSizeLimit: true, retainedFileCountLimit: 15).CreateLogger();
                Logger = logConfig;
                App.Logger = logConfig;
            }
        }

        public static void SetLogLevel(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }


    }
}
