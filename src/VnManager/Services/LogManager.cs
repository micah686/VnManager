
using System;
using System.Collections.Generic;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using Serilog;

namespace VnManager.Services
{
    internal class LogManager
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

    public class SerilogFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            string datestr = logEvent.Timestamp.Date.ToLongDateString();
            string timestr = logEvent.Timestamp.DateTime.ToLongTimeString();
            if (LogManager.LogLevel == LogLevel.Normal)
            {
                output.Write("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, datestr, timestr, logEvent.MessageTemplate); //don't log exceptions

            }
            else if (LogManager.LogLevel == LogLevel.Debug)
            {
                if (logEvent.Exception != null)
                {
                    output.Write("[ {0} | {1} {2}] Msg: {3}\n Ex: {4}", logEvent.Level, datestr, timestr, logEvent.MessageTemplate, logEvent.Exception);
                }
                else
                {
                    output.Write("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, datestr, timestr, logEvent.MessageTemplate);
                }

            }
            else if (LogManager.LogLevel == LogLevel.Verbose)
            {
                string outstr = string.Empty;
                string s1 = string.Empty;
                string s2 = string.Empty;
                string s3 = string.Empty;
                Exception ex = logEvent.Exception;
                s1 = string.Format("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, datestr, timestr, logEvent.MessageTemplate);
                outstr += s1;
                if (logEvent.Exception != null)
                {
                    s2 = string.Format("Ex Message: {0}\nStackTrace: {1}\nInner Ex: {2}\nSource: {3}\nData: {4}\nHResult: {5}\nTargetSite: {6}\n\n", ex.Message, ex.StackTrace, ex.InnerException, ex.Source, ex.Data, ex.HResult, ex.TargetSite);
                }
                outstr += s2;
                if (logEvent.Properties.Count > 0)
                {
                    foreach (var item in logEvent.Properties)
                    {
                        s3 += string.Format("Prop Key: {0} Value: {1}\n", item.Key, item.Value);
                    }
                }
                outstr += s3;
                output.Write("{0}\n", outstr);

            }
            else
            {
                //should not reach here
                throw new NotImplementedException();
            }
        }
    }

    public enum LogLevel
    {
        Normal,
        Debug,
        Verbose
    }
}
