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
                    output.Write("[ {0} | {1} {2}] Msg: {3}\n Ex: {4}", logEvent.Level, datestr, timestr, logEvent.MessageTemplate, logEvent.Exception);
                }
                else
                {
                    output.Write("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, datestr, timestr, logEvent.MessageTemplate);
                }

            }
            else if (LogManager.LogLevel == LogLevel.Verbose)
            {
                StringBuilder outstrBuilder = new StringBuilder();
                Exception ex = logEvent.Exception;
                outstrBuilder.Append(string.Format("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, datestr, timestr, logEvent.MessageTemplate));
                if (logEvent.Exception != null)
                {
                    outstrBuilder.Append(string.Format("Ex Message: {0}\nStackTrace: {1}\nInner Ex: {2}\nSource: {3}\nData: {4}\nHResult: {5}\nTargetSite: {6}\n\n", ex.Message, ex.StackTrace, ex.InnerException, ex.Source, ex.Data, ex.HResult, ex.TargetSite));                    
                }
                if (logEvent.Properties.Count > 0)
                {
                    foreach (var item in logEvent.Properties)
                    {
                        outstrBuilder.Append(string.Format("Prop Key: {0} Value: {1}\n", item.Key, item.Value));
                    }
                }
                output.Write("{0}\n", outstrBuilder.ToString());
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
