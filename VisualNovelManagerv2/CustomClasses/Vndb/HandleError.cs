using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VndbSharp.Interfaces;
using VndbSharp.Models.Errors;

namespace VisualNovelManagerv2.CustomClasses.Vndb
{
    public class HandleError
    {
        public static void HandleErrors(IVndbError error)
        {
            if (error is MissingError missing)
            {
                Debug.WriteLine($"A Missing Error occured, the field \"{missing.Field}\" was missing.");
            }
            else if (error is BadArgumentError badArg)
            {
                Debug.WriteLine($"A BadArgument Error occured, the field \"{badArg.Field}\" is invalid.");
            }
            else if (error is ThrottledError throttled)
            {
                var minSeconds = (DateTime.Now - throttled.MinimumWait).TotalSeconds; // Not sure if this is correct
                var fullSeconds = (DateTime.Now - throttled.FullWait).TotalSeconds; // Not sure if this is correct
                Debug.WriteLine(
                    $"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
                    $"and preferably \"{fullSeconds}\" before issuing commands.");
                TimeSpan ts = new TimeSpan(0,0,0,(int)fullSeconds);
                Thread.Sleep(ts);
            }
            else if (error is GetInfoError getInfo)
            {
                Debug.WriteLine($"A GetInfo Error occured, the flag \"{getInfo.Flag}\" is not valid on the issued command.");
            }
            else if (error is InvalidFilterError invalidFilter)
            {
                Debug.WriteLine(
                    $"A InvalidFilter Error occured, the filter combination of \"{invalidFilter.Field}\", " +
                    $"\"{invalidFilter.Operator}\", \"{invalidFilter.Value}\" is not a valid combination.");
            }
            else
            {
                Debug.WriteLine($"A {error.Type} Error occured.");
            }
            Debug.WriteLine($"Message: {error.Message}");
        }
    }
}
