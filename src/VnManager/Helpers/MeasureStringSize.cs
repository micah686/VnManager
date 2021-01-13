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
