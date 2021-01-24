using System;
using System.Globalization;
using VndbSharp.Models.Common;
using VnManager.Helpers;

namespace VnManager.Converters
{
    public static class SimpleDateConverter
    {
        /// <summary>
        /// Converts a SimpleDate to a string, removing empty entries
        /// </summary>
        /// <param name="birthday">Birthday as a SimpleDate</param>
        /// <returns></returns>
        public static string ConvertSimpleDate(SimpleDate birthday)
        {
            string day = birthday?.Day == null ? string.Empty : birthday.Day.ToString();

            string month = birthday?.Day == null ? string.Empty : DateTimeFormatInfo.InvariantInfo.GetMonthName(Convert.ToInt32(birthday.Month, CultureInfo.InvariantCulture));

                
            if (month == string.Empty)
            {
                month = string.Empty;
                day = string.Empty;
            }
            
            var year = birthday?.Year == null ? string.Empty : birthday.Year.ToString();
            var formatted = $"{month} {day} {year}";
            formatted = NormalizeWhiteSpace.FixWhiteSpace(formatted);
            return formatted;
        }
    }
}
