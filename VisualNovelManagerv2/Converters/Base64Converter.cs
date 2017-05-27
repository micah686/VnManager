using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace VisualNovelManagerv2.Converters
{
    public class Base64Converter
    {
        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to base 64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static string BitmapToBase64(BitmapImage bi)
        {
            MemoryStream ms = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bi));
            encoder.Save(ms);
            byte[] bitmapdata = ms.ToArray();

            return Convert.ToBase64String(bitmapdata);
        }

        public static BitmapImage GetBitmapImageFromBytes(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);
            BitmapImage btm;
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                btm = new BitmapImage();
                btm.BeginInit();
                btm.StreamSource = ms;
                // Below code for caching is crucial.
                btm.CacheOption = BitmapCacheOption.OnLoad;
                btm.EndInit();
                btm.Freeze();
            }
            return btm;
        }
    }
}
