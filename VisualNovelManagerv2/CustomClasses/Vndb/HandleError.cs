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
        public static void HandleErrors(IVndbError error, int? counter)
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
                var minSeconds = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                var fullSeconds = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                //var minSeconds = (throttled.MinimumWait -DateTime.Now).TotalSeconds; // Not sure if this is correct
                //var fullSeconds = (throttled.FullWait - DateTime.Now).TotalSeconds; // Not sure if this is correct
                Debug.WriteLine(
                    $"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
                    $"and preferably \"{fullSeconds}\" before issuing commands.\n" +
                    $"Use the overloaded HandleErrors with the counter to wait for the throttled time");                
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

        public static async void HandleErrors(IVndbError error, int counter)
        {
            if (error is ThrottledError throttled)
            {
                try
                {
                    if (throttled.MinimumWait.Year < 2000)
                    {
                        return;
                    }
                    Debug.WriteLine("minsec: "+(throttled.MinimumWait - DateTime.Now).TotalSeconds);
                    Debug.WriteLine("maxsec: "+ (throttled.FullWait - DateTime.Now).TotalSeconds);
                    var minSeconds = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                    var fullSeconds = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                    Debug.WriteLine(
                        $"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
                        $"and preferably \"{fullSeconds}\" before issuing commands.");

                    double sleepTime = 0;
                    //set seconds to sleep
                    if (counter == 0)
                    {
                        sleepTime = minSeconds.TotalSeconds;
                    }
                    else if (counter >= 1)
                    {
                        sleepTime = (minSeconds.TotalSeconds * counter);
                    }
                    else
                    {
                        sleepTime = 5;
                    }
                    //make sure sleepTime doesn't go above the maximum amount of seconds needed
                    if (sleepTime > fullSeconds.TotalSeconds)
                    {
                        sleepTime = fullSeconds.TotalSeconds;
                    }

                    if (sleepTime >= 0)
                    {
                        int sleep = Convert.ToInt32(sleepTime);
                        //Thread.Sleep(sleep);
                        await Task.Delay(sleep);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                

            }
        }
    }
}
