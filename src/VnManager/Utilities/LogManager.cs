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

        internal static readonly ILogger Logger = new LoggerConfiguration().WriteTo.File(new SerilogFormatter(),
            string.Format(@"{0}\Data\logs\{1}-{2}-{3}_{4}.log", Globals.DirectoryPath, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, LogLevel.ToString()), fileSizeLimitBytes: 500000, rollOnFileSizeLimit: true, retainedFileCountLimit: 15).CreateLogger();


        //private readonly LogManager _logManager;
        //public LogManager(LogManager logManager)
        //{
        //    _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
        //    Logger.Information("Logger Startup Initialized");
        //}

        public static void SetLogLevel(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }


    }
}
