using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace VnManager.Converters
{
    public static class CsvConverter
    {
        public static string ConvertToCsv(ReadOnlyCollection<string> input)
        {
            return input != null ? string.Join(",", input) : null;
        }
    }
}
