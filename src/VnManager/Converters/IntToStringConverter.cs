using System;
using System.Globalization;
using System.Windows.Data;

namespace VnManager.Converters
{
    /// <summary>
    /// XAML Converter for converting an int to a string
    /// </summary>
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.TryParse((string)value, out var ret) ? ret : 0;
        }
    }
}
