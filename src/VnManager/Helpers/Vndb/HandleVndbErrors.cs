using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VndbSharp.Interfaces;
using VndbSharp.Models.Errors;
using VnManager.Utilities;

namespace VnManager.Helpers.Vndb
{
    public class HandleVndbErrors
    {
        public static void HandleErrors(IVndbError error, int counter)
        {
            if (error is MissingError missing)
            {
                Debug.WriteLine($"A Missing Error occured, the field \"{missing.Field}\" was missing.");
                LogManager.Logger.Warning($"A Missing Error occured, the field \"{missing.Field}\" was missing.");
            }
            else if (error is BadArgumentError badArg)
            {
                LogManager.Logger.Warning($"A BadArgument Error occured, the field \"{badArg.Field}\" is invalid.");
            }
            else if (error is ThrottledError throttled)
            {
                try
                {
                    if (throttled.MinimumWait.Year < 2000)
                    {
                        return;
                    }
                    var minsec = (throttled.MinimumWait - DateTime.Now).TotalSeconds;
                    var maxsec = (throttled.FullWait - DateTime.Now).TotalSeconds;
                    var minSeconds = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                    var fullSeconds = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                    TimeSpan timeSpan;
                    LogManager.Logger.Warning($"minsec {0}, maxsec: {1}\nA Throttled Error occured, you need to wait at minimum {2} seconds and preferably {4} before issuing commands.", minsec, maxsec, minSeconds, fullSeconds);

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
                        LogManager.Logger.Warning($"Please wait {timeSpan.TotalMinutes} minutes and {timeSpan.TotalSeconds} seconds");
                        Thread.Sleep(timeSpan);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error(ex, "Throttled logic failed");
                    throw;
                }
            }
            else if (error is GetInfoError getInfo)
            {
                LogManager.Logger.Warning($"A GetInfo Error occured, the flag {0} is not valid on the issued command.", getInfo.Flag);
            }
            else if (error is InvalidFilterError invalidFilter)
            {
                LogManager.Logger.Warning($"A InvalidFilter Error occured, the filter combination of {0}, {1}, {2} is not a valid combination.", invalidFilter.Field, invalidFilter.Operator, invalidFilter.Value);
            }
            else if (error is BadAuthenticationError badAuthentication)
            {
                LogManager.Logger.Warning($"A BadAuthenticationError occured. This is caused by an incorrect username or pasword.\nMessage: {badAuthentication.Message}");
            }
            else
            {
                LogManager.Logger.Warning($"A {error.Type} Error occured.");
            }
            LogManager.Logger.Warning($"Message: {error.Message}");
        }
    }
}
