// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace VnManager.Converters
{
    /// <summary>
    /// XAML Converter for inverting the visibility state
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class InverseVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("The target must be a 'Visibility' type");
            }
            if (value == null)
            {
                throw new InvalidOperationException("The target cannot be null");
            }

            var vis = (Visibility) value;
            return vis switch
            {
                Visibility.Collapsed => Visibility.Visible,
                Visibility.Hidden => Visibility.Visible,
                Visibility.Visible => Visibility.Collapsed,
                _ => throw new NotSupportedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
