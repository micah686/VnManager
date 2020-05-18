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
    public static class HandleVndbErrors
    {
        public static void HandleErrors(IVndbError error, int counter)
        {
            switch (error)
            {
                case MissingError missing:
                    Debug.WriteLine($"A Missing Error occured, the field {missing.Field} was missing.");
                    App.Logger.Warning($"A Missing Error occured, the field {missing.Field} was missing.");
                    break;
                case BadArgumentError badArg:
                    Debug.WriteLine($"A BadArgument Error occured, the field {badArg.Field} is invalid.");
                    App.Logger.Warning($"A BadArgument Error occured, the field {badArg.Field} is invalid.");
                    break;
                case ThrottledError throttled:
                    ThrottledCheck(throttled, counter);
                    break;
                case GetInfoError getInfo:
                    Debug.WriteLine($"A GetInfo Error occured, the flag {getInfo.Flag} is not valid on the issued command.");
                    App.Logger.Warning($"A GetInfo Error occured, the flag {getInfo.Flag} is not valid on the issued command.");
                    break;
                case InvalidFilterError invalidFilter:
                    Debug.WriteLine($"A InvalidFilter Error occured, the filter combination of {invalidFilter.Field}, {invalidFilter.Operator}, {invalidFilter.Value} is not a valid combination.");
                    App.Logger.Warning($"A InvalidFilter Error occured, the filter combination of {invalidFilter.Field}, {invalidFilter.Operator}, {invalidFilter.Value} is not a valid combination.");
                    break;
                case BadAuthenticationError badAuthentication:
                    Debug.WriteLine($"A BadAuthenticationError occured. This is caused by an incorrect username or pasword.\nMessage: {badAuthentication.Message}");
                    App.Logger.Warning($"A BadAuthenticationError occured. This is caused by an incorrect username or pasword.\nMessage: {badAuthentication.Message}");
                    break;
                default:
                    Debug.WriteLine($"A {error.Type} Error occured.\nMessage: {error.Message}");
                    App.Logger.Warning($"A {error.Type} Error occured.\nMessage: {error.Message}");
                    break;
            }
        }

        private static void ThrottledCheck(ThrottledError throttled, int counter)
        {
            try
            {
                if (throttled.MinimumWait.Year < 2000) return;
                var minsec = (throttled.MinimumWait - DateTime.Now).TotalSeconds;
                var maxsec = (throttled.FullWait - DateTime.Now).TotalSeconds;
                var minSeconds = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                var fullSeconds = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds); // Not sure if this is correct
                TimeSpan timeSpan;
                Debug.WriteLine($"minsec {minsec}, maxsec: {maxsec}\nA Throttled Error occured, you need to wait at minimum {minSeconds} seconds and preferably {fullSeconds} before issuing commands.");
                App.Logger.Warning($"minsec {minsec}, maxsec: {maxsec}\nA Throttled Error occured, you need to wait at minimum {minSeconds} seconds and preferably {fullSeconds} before issuing commands.");

                if (counter == 0) 
                    timeSpan = TimeSpan.FromSeconds(minSeconds.TotalSeconds);
                else if (counter >= 1) 
                    timeSpan = TimeSpan.FromSeconds(minSeconds.TotalSeconds * counter);
                else 
                    timeSpan = TimeSpan.FromSeconds(5);

                if (timeSpan > fullSeconds)
                {
                    timeSpan = TimeSpan.FromSeconds(fullSeconds.TotalSeconds);
                }

                if (timeSpan >= new TimeSpan(0, 0, 0, 0, 0))
                {
                    App.Logger.Warning($"Please wait {timeSpan.TotalMinutes} minutes and {timeSpan.TotalSeconds} seconds");
                    Thread.Sleep(timeSpan); //does this need to be Thread.Sleep or Task.Delay?
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Throttled logic failed");
                App.Logger.Error(ex, "Throttled logic failed");
                throw;
            }
        }
    }
}
