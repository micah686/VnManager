using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using VndbSharp.Models.Common;
using VnManager.Helpers;

namespace VnManager.Converters
{
    public static class BirthdayConverter
    {
        /// <summary>
        /// Converts a SimpleDate to a string, removing empty entries
        /// </summary>
        /// <param name="birthday">Birthday as a SimpleDate</param>
        /// <returns></returns>
        public static string ConvertBirthday(SimpleDate birthday)
        {
            string month;
            string day = string.Empty;
            if (birthday == null) return string.Empty;
            if (birthday.Month == null)
            {
                month = string.Empty;
                day = string.Empty;
            }
            else
            {
                month = DateTimeFormatInfo.InvariantInfo.GetMonthName(Convert.ToInt32(birthday.Month, CultureInfo.InvariantCulture));
                
            }

            var year = birthday.Year == null ? string.Empty : birthday.Year.ToString();
            var formatted = $"{month} {day} {year}";
            formatted = NormalizeWhiteSpace.FixWhiteSpace(formatted);
            return formatted;
        }
    }
}
