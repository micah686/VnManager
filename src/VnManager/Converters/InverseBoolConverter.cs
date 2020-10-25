using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace VnManager.Converters
{
    /// <summary>
    /// XAML converter for inverting a boolean value
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");
            if(value == null)
                throw new InvalidOperationException("The target cannot be null");
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
