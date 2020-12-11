using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VnManager.Helpers
{
    public static class MeasureStringSize
    {
        public static double GetMaxStringWidth(string text)
        {
            //FormattedText formattedText = new FormattedText(
            //    text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            //    new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            //    13, Brushes.Black);
            //add 10 for some extra padding

            var typeFace = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal,
                FontWeights.Bold, FontStretches.Normal);
            var fontSize = 12;

            FormattedText formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, typeFace, fontSize, Brushes.Black,
                VisualTreeHelper.GetDpi(new TextBlock()).PixelsPerDip);


            return formattedText.Width;


        }
    }
}
