// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VnManager.Helpers
{
    public static class MeasureStringSize
    {
        public static double GetMaxStringWidth(string text)
        {
            var typeFace = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal,
                FontWeights.Bold, FontStretches.Normal);
            const int fontSize = 12;

            FormattedText formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, typeFace, fontSize, Brushes.Black,
                VisualTreeHelper.GetDpi(new TextBlock()).PixelsPerDip);


            return formattedText.Width;


        }
    }
}
