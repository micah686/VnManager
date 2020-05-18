using System;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using Serilog;
using VnManager.Converters;

namespace VnManager.Utilities
{
    internal static class LogManager
    {
        public static LogLevel LogLevel { get; private set; } = LogLevel.Normal;

        internal static  ILogger Logger = new LoggerConfiguration().WriteTo.File(new SerilogFormatter(), string.Format(@"{0}\logs\{1}-{2}-{3}_{4}.log", GetConfigDirectory(), DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, LogLevel.ToString()),
                    fileSizeLimitBytes: 500000, rollOnFileSizeLimit: true, retainedFileCountLimit: 15).CreateLogger();


        private static string GetConfigDirectory()
        {
            if (string.IsNullOrEmpty(App.ConfigDirPath))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else
            {
                return App.ConfigDirPath;
            }
        }

        internal static void UpdateLoggerDirectory()
        {
            if (!App.StartupLockout)//disallow updating logger after App has started
            {
                var log = new LoggerConfiguration().WriteTo.File(new SerilogFormatter(), string.Format(@"{0}\logs\{1}-{2}-{3}_{4}.log", GetConfigDirectory(), DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, LogLevel.ToString()),
                    fileSizeLimitBytes: 500000, rollOnFileSizeLimit: true, retainedFileCountLimit: 15).CreateLogger();
                Logger = log;
            }
        }

        public static void SetLogLevel(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }


    }
}
