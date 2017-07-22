using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VisualNovelManagerv2.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class YesNoBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is bool && (bool)value;

            return boolValue ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.ToString() == "Yes";
        }
    }
}
