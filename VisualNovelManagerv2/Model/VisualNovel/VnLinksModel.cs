using System.Windows;
using System.Windows.Media.Imaging;

namespace VisualNovelManagerv2.Model.VisualNovel
{
    public class VnLinksModel: DependencyObject
    {


        public BitmapImage Image
        {
            get { return (BitmapImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(BitmapImage), typeof(VnLinksModel), new PropertyMetadata(null));


    }
}
