using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VndbSharp.Interfaces;
using VndbSharp.Models.Errors;

namespace VisualNovelManagerv2.CustomClasses.Vndb
{
    public class HandleError
    {
        public static async void HandleErrors(IVndbError error, int counter)
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
                try
                {
                    if (throttled.MinimumWait.Year < 2000)
                    {
                        return;
                    }
                    Debug.WriteLine("minsec: " + (throttled.MinimumWait - DateTime.Now).TotalSeconds);
                    Debug.WriteLine("maxsec: " + (throttled.FullWait - DateTime.Now).TotalSeconds);
                    var minSeconds = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                    var fullSeconds = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                    Debug.WriteLine(
                        $"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
                        $"and preferably \"{fullSeconds}\" before issuing commands.");
                    TimeSpan timeSpan;
                    //double sleepTime = 0;
                    //set seconds to sleep
                    if (counter == 0)
                    {
                        timeSpan = TimeSpan.FromSeconds(minSeconds.TotalSeconds);
                    }
                    else if (counter >= 1)
                    {
                        timeSpan = TimeSpan.FromSeconds(minSeconds.TotalSeconds * counter);
                    }
                    else
                    {
                        timeSpan = TimeSpan.FromSeconds(5);
                    }
                    //make sure sleepTime doesn't go above the maximum amount of seconds needed
                    if (timeSpan > fullSeconds)
                    {
                        timeSpan = TimeSpan.FromSeconds(fullSeconds.TotalSeconds);
                    }

                    if (timeSpan >= new TimeSpan(0, 0, 0, 0, 0))
                    {
                        Debug.WriteLine($"Please wait {timeSpan.TotalMinutes} minutes and {timeSpan.TotalSeconds} seconds");
                        Thread.Sleep(timeSpan);
                    }
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    throw;
                }
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
            else if (error is BadAuthenticationError badAuthentication)
            {
                Debug.WriteLine(
                    $"A BadAuthenticationError occured. This is caused by an incorrect username or pasword.\n" +
                    $"Message: {badAuthentication.Message}");
            }
            else
            {
                Debug.WriteLine($"A {error.Type} Error occured.");
            }
            Debug.WriteLine($"Message: {error.Message}");
        }        
    }
}
