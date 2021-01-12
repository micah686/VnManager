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
using VnManager.ViewModels.UserControls;

namespace VnManager.Helpers.Vndb
{
    public static class HandleVndbErrors
    {
        /// <summary>
        /// Handles Vndb API errors, writes the error message to a file, then resets the statusbar
        /// </summary>
        /// <param name="error">Vndb Error</param>
        public static void HandleErrors(IVndbError error)
        {
            if(error == null)
            {
                return;
            }
            switch (error)
            {
                case MissingError missing:
                    Debug.WriteLine($"A Missing Error occurred, the field {missing.Field} was missing.");
                    App.Logger.Warning($"A Missing Error occurred, the field {missing.Field} was missing.");
                    break;
                case BadArgumentError badArg:
                    Debug.WriteLine($"A BadArgument Error occurred, the field {badArg.Field} is invalid.");
                    App.Logger.Warning($"A BadArgument Error occurred, the field {badArg.Field} is invalid.");
                    break;
                case ThrottledError throttled:
                    Debug.WriteLine($"A Throttled Error occurred, use the ThrottledWaitAsync() method to wait for the {throttled.MinimumWait.Second} seconds needed.");
                    App.Logger.Warning($"A Throttled Error occurred, use the ThrottledWaitAsync() method to wait for the {throttled.MinimumWait.Second} seconds needed.");
                    break;
                case GetInfoError getInfo:
                    Debug.WriteLine($"A GetInfo Error occurred, the flag {getInfo.Flag} is not valid on the issued command.");
                    App.Logger.Warning($"A GetInfo Error occurred, the flag {getInfo.Flag} is not valid on the issued command.");
                    break;
                case InvalidFilterError invalidFilter:
                    Debug.WriteLine($"A InvalidFilter Error occurred, the filter combination of {invalidFilter.Field}, {invalidFilter.Operator}, {invalidFilter.Value} is not a valid combination.");
                    App.Logger.Warning($"A InvalidFilter Error occurred, the filter combination of {invalidFilter.Field}, {invalidFilter.Operator}, {invalidFilter.Value} is not a valid combination.");
                    break;
                case BadAuthenticationError badAuthentication:
                    Debug.WriteLine($"A BadAuthenticationError occurred. This is caused by an incorrect username or password.\nMessage: {badAuthentication.Message}");
                    App.Logger.Warning($"A BadAuthenticationError occurred. This is caused by an incorrect username or password.\nMessage: {badAuthentication.Message}");
                    break;
                default:
                    Debug.WriteLine($"A {error.Type} Error occurred.\nMessage: {error.Message}");
                    App.Logger.Warning($"A {error.Type} Error occurred.\nMessage: {error.Message}");
                    break;
            }
            StatusBarViewModel.ResetValues();
        }
        /// <summary>
        /// Waits a specified amount of time if the API is throttled, to allow enough time to send more commands to the API again
        /// </summary>
        /// <param name="throttled">The throttled Error object with the wait times</param>
        /// <param name="counter">How many times in a loop this method has been called</param>
        /// <returns></returns>
        public static async Task ThrottledWaitAsync(ThrottledError throttled, int counter)
        {
            
            var minWait = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds);
            var maxWait = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds);
            Debug.WriteLine($"Vndb API throttled! You need to wait {minWait.Seconds} seconds minimum or {maxWait.Seconds} seconds maximum before issuing new commands\nErrorCounter:{counter}");
            App.Logger.Warning($"Vndb API throttled! You need to wait {minWait.Seconds} seconds minimum or {maxWait.Seconds} seconds maximum before issuing new commands");

            double waitTime = counter == 0 ? minWait.TotalSeconds : TimeSpan.FromSeconds(5).TotalSeconds;            
            if (counter >= 1)
            {
                waitTime = waitTime > maxWait.TotalSeconds ? maxWait.TotalSeconds : minWait.TotalSeconds + 5;
            }
            waitTime = Math.Abs(waitTime);
            var timeSpan = TimeSpan.FromSeconds(waitTime);
            App.Logger.Warning($"Please wait {timeSpan.TotalMinutes} minutes and {timeSpan.TotalSeconds} seconds");
            await Task.Delay(timeSpan);
            
        }
    }
}
