using System;
using Serilog;

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
        
        /// <summary>
        /// Gets the directory of the config file
        /// </summary>
        /// <returns></returns>
        private static string GetConfigDirectory()
        {
            return string.IsNullOrEmpty(App.ConfigDirPath) ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) : App.ConfigDirPath;
        }

        /// <summary>
        /// Sets up the initial logger, and what directory it should put logs into
        /// </summary>
        /// <returns></returns>
        private static ILogger SetInitialLogger()
        {
            var logConfig = new LoggerConfiguration().WriteTo.File(new SerilogFormatter(), $@"{GetConfigDirectory()}\logs\{DateTime.Now:dd-MM-yyyy}_{LogLevel.ToString()}.log",
                fileSizeLimitBytes: 500000, rollOnFileSizeLimit: true, retainedFileCountLimit: 15).CreateLogger();
            return logConfig;
        }

        /// <summary>
        /// Lets you update the directory that the logger uses.
        /// Note: Once the app has started up, this can no longer be changed
        /// </summary>
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

        /// <summary>
        /// Sets the log level for the logger
        /// </summary>
        /// <param name="logLevel">New log level to use</param>
        public static void SetLogLevel(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }


    }
}
