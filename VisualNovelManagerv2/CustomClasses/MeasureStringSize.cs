using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VisualNovelManagerv2.CustomClasses
{
    public class MeasureStringSize
    {
        public static double GetMaxStringWidth(string text)
        {
            FormattedText formattedText = new FormattedText(
                text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                13, Brushes.Black);
            //add 10 for some extra padding
            return (formattedText.Width + 10);
        }
    }
}
