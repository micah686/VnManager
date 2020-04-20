using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;

namespace VnManager.Converters
{
    public class SerilogFormatter: ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            //output.Write("Timestamp - {0} | Level - {1} | Message {2} {3}", logEvent.Timestamp.DateTime.ToLongDateString(), logEvent.Level, logEvent.MessageTemplate, output.NewLine);
            //if (logEvent.Exception != null)
            //{
            //    output.Write("Exception - {0}", logEvent.Exception);
            //}

            string datestr = logEvent.Timestamp.Date.ToLongDateString();
            string timestr = logEvent.Timestamp.DateTime.ToLongTimeString();
            if (Globals.Loglevel == LogLevel.Normal)
            {
                output.Write("[ {0} | {1} {2}] Msg: {3}\n", logEvent.Level, datestr, timestr, logEvent.MessageTemplate); //don't log exceptions

            }
            else if (Globals.Loglevel == LogLevel.Debug)
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
            else if (Globals.Loglevel == LogLevel.Verbose)
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
}
