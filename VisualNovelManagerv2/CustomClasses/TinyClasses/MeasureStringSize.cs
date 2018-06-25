using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace VisualNovelManagerv2.CustomClasses.TinyClasses
{
    public class MeasureStringSize
    {
        public static double GetMaxStringWidth(string text)
        {
            FormattedText formattedText = new FormattedText(
                text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                13, Brushes.Black);
            //add 10 for some extra padding
            return (formattedText.Width + 25);
        }
    }
}
