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
                string days = string.Empty;
                if (timeSpan.Duration().Days > 0)
                {
                    days = $"{timeSpan.Days} day";
                    if (timeSpan.Days != 1)
                    {
                        days += "s";
                    }
                    days += ", ";
                }


                string hours = string.Empty;
                if (timeSpan.Duration().Hours > 0)
                {
                    hours = $"{timeSpan.Hours} hour";
                    if (timeSpan.Hours != 1)
                    {
                        hours += "s";
                    }

                    hours += ", ";
                }

                string minutes = string.Empty;
                if (timeSpan.Duration().Minutes > 0)
                {
                    minutes = $"{timeSpan.Minutes} minute";
                    if (timeSpan.Minutes != 1)
                    {
                        minutes = minutes + "s";
                    }

                    minutes += " ";
                }

                output = $"{days}{hours}{minutes}";
            }

            return output;
        }
    }
}
