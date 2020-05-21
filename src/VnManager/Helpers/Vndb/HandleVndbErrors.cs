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
                    Debug.WriteLine($"A Throttled Error occured, use the ThrottledWait() method to wait for the {throttled.MinimumWait.Second} seconds needed.");
                    App.Logger.Warning($"A Throttled Error occured, use the ThrottledWait() method to wait for the {throttled.MinimumWait.Second} seconds needed.");
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
        public static async Task ThrottledWait(ThrottledError throttled, int counter)
        {
            
            var minWait = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds);
            var maxwait = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds);
            Debug.WriteLine($"Vndb API throttled! You need to wait {minWait.Seconds} seconds minimum or {maxwait.Seconds} seconds maximum before issuing new commands\nErrorCounter:{counter}");
            App.Logger.Warning($"Vndb API throttled! You need to wait {minWait.Seconds} seconds minimum or {maxwait.Seconds} seconds maximum before issuing new commands");

            double waitime = counter == 0 ? minWait.TotalSeconds : TimeSpan.FromSeconds(5).TotalSeconds;            
            if (counter >= 1)
            {
                waitime = waitime > maxwait.TotalSeconds ? maxwait.TotalSeconds : minWait.TotalSeconds + 5;
            }
            waitime = Math.Abs(waitime);
            var timeSpan = TimeSpan.FromSeconds(waitime);
            App.Logger.Warning($"Please wait {timeSpan.TotalMinutes} minutes and {timeSpan.TotalSeconds} seconds");
            await Task.Delay(timeSpan);
            
        }
    }
}
