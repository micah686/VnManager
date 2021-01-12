using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VnManager.Converters
{
    /// <summary>
    /// XAML converter for converting a Enum to a Bool value
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString is null)
            {
                return DependencyProperty.UnsetValue;
            }


            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }


            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }
            
            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            return parameterString is null ? DependencyProperty.UnsetValue : Enum.Parse(targetType, parameterString);
        }
    }
}
