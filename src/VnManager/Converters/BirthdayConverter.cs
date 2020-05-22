using System;
using System.Collections.Generic;
using System.Text;
using VndbSharp.Models.Common;

namespace VnManager.Converters
{
    public class BirthdayConverter
    {
        public string ConvertBirthday(SimpleDate birthday)
        {
            string formatted = string.Empty;
            if (birthday == null) return formatted;
            if (birthday.Month == null) return birthday.Month == null ? birthday.Day.ToString() : string.Empty;
            string month = System.Globalization.DateTimeFormatInfo.InvariantInfo.GetMonthName(Convert.ToInt32(birthday.Month));
            formatted = $"{month} {birthday.Day}";
            return formatted;
        }
    }
}
