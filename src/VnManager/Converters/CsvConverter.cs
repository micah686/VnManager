// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace VnManager.Converters
{
    public static class CsvConverter
    {
        /// <summary>
        /// Converts a List of strings to a CSV string
        /// </summary>
        /// <param name="input">Collection of strings to convert to csv</param>
        /// <returns></returns>
        public static string ConvertToCsv(IEnumerable<string> input)
        {
            return input != null ? string.Join(",", input) : null;
        }
    }

    /// <summary>
    /// Converts an Enumerable list of strings to a CSV
    /// </summary>
    public class BindingCsvConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var values = (IEnumerable<string>) value;
            return values != null ? string.Join(",", values) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
