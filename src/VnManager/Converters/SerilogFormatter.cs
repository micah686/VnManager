using System;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using Serilog;
using VnManager.Utilities;
using System.Text;

namespace VnManager.Converters
{
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
                    output.Write($"[ {logEvent.Level} | {datestr} {timestr}] Msg: {logEvent.MessageTemplate}\n Ex: {logEvent.Exception}");
                }
                else
                {
                    output.Write($"[ {logEvent.Level} | {datestr} {timestr}] Msg: {logEvent.MessageTemplate}\n");
                }

            }
            else //should be LogLevel.Verbose
            {
                StringBuilder outstrBuilder = new StringBuilder();
                Exception ex = logEvent.Exception;
                outstrBuilder.Append($"[ {logEvent.Level} | {datestr} {timestr}] Msg: {logEvent.MessageTemplate}\n");
                if (logEvent.Exception != null)
                {
                    outstrBuilder.Append(
                        $"Ex Message: {ex.Message}\nStackTrace: {ex.StackTrace}\nInner Ex: {ex.InnerException}\nSource: {ex.Source}\nData: {ex.Data}\nHResult: {ex.HResult}\nTargetSite: {ex.TargetSite}\n\n");                    
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

    public enum LogLevel
    {
        Normal,
        Debug,
        Verbose
    }
}
