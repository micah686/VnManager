using System;
using System.Linq;
using System.Windows.Data;

namespace VnManager.Converters
{
    /// <summary>
    /// XAML Converter for allowing for AND operator between 2 values
    /// </summary>
    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.OfType<IConvertible>().All(System.Convert.ToBoolean);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
