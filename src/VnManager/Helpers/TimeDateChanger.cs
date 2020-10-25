using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VnManager.Helpers
{
    public static class TimeDateChanger
    {
        /// <summary>
        /// Convert a DateTime to a more human readable date
        /// </summary>
        /// <param name="dateTime">Date you want to convert</param>
        /// <returns></returns>
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
                    
                    output = dateTime.Date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                }
            }

            return output;
        }

        /// <summary>
        /// Convert TimeSpan to a Human Time
        /// </summary>
        /// <param name="timeSpan">Timespan to convert</param>
        /// <returns></returns>
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
                string days = GetHumanDays(timeSpan);
                string hours = GetHumanHours(timeSpan);
                string minutes = GetHumanMinutes(timeSpan);
                string weeks = GetHumanWeeks(timeSpan);

                output = $"{weeks}{days}{hours}{minutes}";
            }

            return output;
        }

        /// <summary>
        /// Get weeks out of a Timespan
        /// </summary>
        /// <param name="timeSpan">Timespan to parse</param>
        /// <returns></returns>
        private static string GetHumanWeeks(TimeSpan timeSpan)
        {
            string weeks = string.Empty;
            var weekCount = (timeSpan.Days - (timeSpan.Days % 7)) / 7;
            if (weekCount <= 0) return weeks;
            weeks = $"{weekCount} week";
            if (weekCount != 1)
            {
                weeks += "s";
            }
            weeks += ", ";
            return weeks;
        }

        /// <summary>
        /// Get days out of a Timespan
        /// </summary>
        /// <param name="timeSpan">Timespan to parse</param>
        /// <returns></returns>
        private static string GetHumanDays(TimeSpan timeSpan)
        {
            string days = string.Empty;
            var dayCount = timeSpan.Days % 7;
            if (dayCount <= 0) return days;
            days = $"{dayCount} day";
            if (dayCount != 1)
            {
                days += "s";
            }
            days += ", ";
            return days;
        }

        /// <summary>
        /// Get hours out of a Timespan
        /// </summary>
        /// <param name="timeSpan">Timespan to parse</param>
        /// <returns></returns>
        private static string GetHumanHours(TimeSpan timeSpan)
        {
            string hours = string.Empty;
            if (timeSpan.Duration().Hours <= 0) return hours;
            hours = $"{timeSpan.Hours} hour";
            if (timeSpan.Hours != 1)
            {
                hours += "s";
            }

            hours += ", ";
            return hours;
        }

        /// <summary>
        /// Get seconds out of a Timespan
        /// </summary>
        /// <param name="timeSpan">Timespan to parse</param>
        /// <returns></returns>
        private static string GetHumanMinutes(TimeSpan timeSpan)
        {
            string minutes = string.Empty;
            if (timeSpan.Duration().Minutes <= 0) return minutes;
            minutes = $"{timeSpan.Minutes} minute";
            if (timeSpan.Minutes != 1)
            {
                minutes = minutes + "s";
            }

            minutes += " ";
            return minutes;
        }

        
    }
}
