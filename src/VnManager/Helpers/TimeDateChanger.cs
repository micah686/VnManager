using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VnManager.Helpers
{
    public static class TimeDateChanger
    {
        public static string GetHumanDate(DateTime dateTime)
        {
            string output;
            if (dateTime == DateTime.MinValue)
            {
                output = "Never";
            }
            else
            {
                if ((Convert.ToDateTime(dateTime) - DateTime.Today).Days > -7)
                {
                    if (dateTime == DateTime.Today)
                    {
                        output = "Today";
                    }
                    else if ((Convert.ToDateTime(dateTime) - DateTime.Today).Days > -2 &&
                             (Convert.ToDateTime(dateTime) - DateTime.Today).Days < 0)
                    {
                        output = "Yesterday";
                    }
                    else
                    {
                        output = dateTime.DayOfWeek.ToString();
                    }
                }
                else
                {
                    output = dateTime.Date.ToShortDateString();
                }
            }

            return output;
        }

        public static string GetHumanTime(TimeSpan timeSpan)
        {
            string output;
            if (timeSpan == TimeSpan.Zero)
            {
                output = "Never";
            }
            else if (timeSpan == new TimeSpan(0, 0, 0, 60))
            {
                output = "Less than a minute";
            }
            else
            {
                output = $"{(timeSpan.Duration().Days > 0 ? $"{timeSpan.Days:0} day{(timeSpan.Days == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                         $"{(timeSpan.Duration().Hours > 0 ? $"{timeSpan.Hours:0} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                         $"{(timeSpan.Duration().Minutes > 0 ? $"{timeSpan.Minutes:0} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")} " : string.Empty)}";
            }

            return output;
        }
    }
}
