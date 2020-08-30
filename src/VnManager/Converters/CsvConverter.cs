using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
}
