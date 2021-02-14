// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VnManager.Helpers
{
    public static class MeasureStringSize
    {
        //TODO: Is this class even used?
        /// <summary>
        /// Get the width in pixels of certain text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static double GetMaxStringWidth(string text)
        {
            try
            {
                var typeFace = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal,
                    FontWeights.Bold, FontStretches.Normal);
                const int fontSize = 12;

                FormattedText formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, typeFace, fontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(new TextBlock()).PixelsPerDip);


                return formattedText.Width;
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to get max string size");
                return -1;
            }


        }
    }
}
