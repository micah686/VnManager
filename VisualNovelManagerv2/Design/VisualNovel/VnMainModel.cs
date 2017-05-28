using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VisualNovelManagerv2.Design.VisualNovel
{
    public class VnMainModel: DependencyObject
    {


        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(VnMainModel), new PropertyMetadata("binding name"));



        public BitmapSource VnIcon
        {
            get { return (BitmapSource)GetValue(VnIconProperty); }
            set { SetValue(VnIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VnIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VnIconProperty =
            DependencyProperty.Register("VnIcon", typeof(BitmapSource), typeof(VnMainModel), new PropertyMetadata(null));



        public string Original
        {
            get { return (string)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Original.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginalProperty =
            DependencyProperty.Register("Original", typeof(string), typeof(VnMainModel), new PropertyMetadata(null));



        public string Released
        {
            get { return (string)GetValue(ReleasedProperty); }
            set { SetValue(ReleasedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Released.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReleasedProperty =
            DependencyProperty.Register("Released", typeof(string), typeof(VnMainModel), new PropertyMetadata(null));



        public BitmapImage Languages
        {
            get { return (BitmapImage)GetValue(LanguagesProperty); }
            set { SetValue(LanguagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Languages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LanguagesProperty =
            DependencyProperty.Register("Languages", typeof(BitmapImage), typeof(VnMainModel), new PropertyMetadata(null));



        public BitmapImage OriginalLanguages
        {
            get { return (BitmapImage)GetValue(OriginalLanguagesProperty); }
            set { SetValue(OriginalLanguagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OriginalLanguages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginalLanguagesProperty =
            DependencyProperty.Register("OriginalLanguages", typeof(BitmapImage), typeof(VnMainModel), new PropertyMetadata(null));



        public string Platforms
        {
            get { return (string)GetValue(PlatformsProperty); }
            set { SetValue(PlatformsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Platforms.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlatformsProperty =
            DependencyProperty.Register("Platforms", typeof(string), typeof(VnMainModel), new PropertyMetadata(null));



        public string Aliases
        {
            get { return (string)GetValue(AliasesProperty); }
            set { SetValue(AliasesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Aliases.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AliasesProperty =
            DependencyProperty.Register("Aliases", typeof(string), typeof(VnMainModel), new PropertyMetadata(null));



        public string Length
        {
            get { return (string)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Length.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LengthProperty =
            DependencyProperty.Register("Length", typeof(string), typeof(VnMainModel), new PropertyMetadata(null));



        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(VnMainModel), new PropertyMetadata(null));



        public BitmapSource Image
        {
            get { return (BitmapSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(BitmapSource), typeof(VnMainModel), new PropertyMetadata(null));



        public double Popularity
        {
            get { return (double)GetValue(PopularityProperty); }
            set { SetValue(PopularityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Popularity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopularityProperty =
            DependencyProperty.Register("Popularity", typeof(double), typeof(VnMainModel), new PropertyMetadata(0.0));



        public int Rating
        {
            get { return (int)GetValue(RatingProperty); }
            set { SetValue(RatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register("Rating", typeof(int), typeof(VnMainModel), new PropertyMetadata(0));




    }
}
