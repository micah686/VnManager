using System;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using Serilog;
using VnManager.Utilities;
using System.Text;

namespace VnManager.Utilities
{
    /// <summary>
    /// Class that configures the formatting for Serilog
    /// </summary>
    public class SerilogFormatter : ITextFormatter
    {
        /// <summary>
        /// Formats the logs that serilog uses
        /// </summary>
        /// <param name="logEvent">The logEvent that was generated with Serilog</param>
        /// <param name="output">The output string for the log file</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            string dateStr = logEvent.Timestamp.Date.ToLongDateString();
            string timeStr = logEvent.Timestamp.DateTime.ToLongTimeString();
            if (LogManager.LogLevel == LogLevel.Normal)
            {
                output.Write("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, dateStr, timeStr, logEvent.MessageTemplate); //don't log exceptions

            }
            else if (LogManager.LogLevel == LogLevel.Debug)
            {
                if (logEvent.Exception != null)
                {
                    output.Write($"[ {logEvent.Level} | {dateStr} {timeStr}] Msg: {logEvent.MessageTemplate}\n Ex: {logEvent.Exception}");
                }
                else
                {
                    output.Write($"[ {logEvent.Level} | {dateStr} {timeStr}] Msg: {logEvent.MessageTemplate}\n");
                }

            }
            else //should be LogLevel.Verbose
            {
                StringBuilder outstrBuilder = new StringBuilder();
                Exception ex = logEvent.Exception;
                outstrBuilder.Append($"[ {logEvent.Level} | {dateStr} {timeStr}] Msg: {logEvent.MessageTemplate}\n");
                if (logEvent.Exception != null)
                {
                    outstrBuilder.Append($"Ex Message: {ex.Message}\nStackTrace: {ex.StackTrace}\nInner Ex: {ex.InnerException}\nSource: " +
                                         $"{ex.Source}\nData: {ex.Data}\nHResult: {ex.HResult}\nTargetSite: {ex.TargetSite}\n\n");
                }
                if (logEvent.Properties.Count > 0)
                {
                    foreach (var item in logEvent.Properties)
                    {
                        outstrBuilder.Append($"Prop Key: {item.Key} Value: {item.Value}\n");
                    }
                }
                output.Write("{0}\n", outstrBuilder.ToString());
            }
        }
    }
    /// <summary>
    /// What level of logs should be used
    /// </summary>
    public enum LogLevel
    {
        Normal,
        Debug,
        Verbose
    }
}
