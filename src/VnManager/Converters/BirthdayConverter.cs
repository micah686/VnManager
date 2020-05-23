﻿using System;
using System.Collections.Generic;
using System.Text;
using VndbSharp.Models.Common;
using VnManager.Helpers;

namespace VnManager.Converters
{
    public class BirthdayConverter
    {
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
                month = System.Globalization.DateTimeFormatInfo.InvariantInfo.GetMonthName(Convert.ToInt32(birthday.Month));
            }

            var year = birthday.Year == null ? string.Empty : birthday.Year.ToString();
            var formatted = $"{month} {day} {year}";
            formatted = NormalizeWhiteSpace.FixWhiteSpace(formatted);
            return formatted;
        }
    }
}
