using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace VisualNovelManagerv2.Design.VisualNovel
{
    public class VnReleaseModel: DependencyObject
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));



        public string OriginalTitle
        {
            get { return (string)GetValue(OriginalTitleProperty); }
            set { SetValue(OriginalTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OriginalName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginalTitleProperty =
            DependencyProperty.Register("OriginalTitle", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));



        public string Released
        {
            get { return (string)GetValue(ReleasedProperty); }
            set { SetValue(ReleasedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Released.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReleasedProperty =
            DependencyProperty.Register("Released", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));



        public string ReleaseType
        {
            get { return (string)GetValue(ReleaseTypeProperty); }
            set { SetValue(ReleaseTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReleaseType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReleaseTypeProperty =
            DependencyProperty.Register("ReleaseType", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));



        public string Patch
        {
            get { return (string)GetValue(PatchProperty); }
            set { SetValue(PatchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Patch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PatchProperty =
            DependencyProperty.Register("Patch", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));


        public string Freeware
        {
            get { return (string)GetValue(FreewareProperty); }
            set { SetValue(FreewareProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Freeware.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FreewareProperty =
            DependencyProperty.Register("Freeware", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));



        public string Doujin
        {
            get { return (string)GetValue(DoujinProperty); }
            set { SetValue(DoujinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Doujin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoujinProperty =
            DependencyProperty.Register("Doujin", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));



        public BitmapImage Languages
        {
            get { return (BitmapImage)GetValue(LanguagesProperty); }
            set { SetValue(LanguagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LanguagesProperty =
            DependencyProperty.Register("Languages", typeof(BitmapImage), typeof(VnReleaseModel), new PropertyMetadata(null));




        public string Website
        {
            get { return (string)GetValue(WebsiteProperty); }
            set { SetValue(WebsiteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Website.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebsiteProperty =
            DependencyProperty.Register("Website", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));




        public FlowDocument Notes
        {
            get { return (FlowDocument)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Notes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(FlowDocument), typeof(VnReleaseModel), new PropertyMetadata(null));




        public int? MinAge
        {
            get { return (int?)GetValue(MinAgeProperty); }
            set { SetValue(MinAgeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinAge.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinAgeProperty =
            DependencyProperty.Register("MinAge", typeof(int?), typeof(VnReleaseModel), new PropertyMetadata(null));



        public UInt64? Gtin
        {
            get { return (UInt64?)GetValue(GtinProperty); }
            set { SetValue(GtinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Gtin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GtinProperty =
            DependencyProperty.Register("Gtin", typeof(UInt64?), typeof(VnReleaseModel), new PropertyMetadata(null));



        public string Catalog
        {
            get { return (string)GetValue(CatalogProperty); }
            set { SetValue(CatalogProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Catalog.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CatalogProperty =
            DependencyProperty.Register("Catalog", typeof(string), typeof(VnReleaseModel), new PropertyMetadata(null));







    }

    public class VnReleaseProducerModel : DependencyObject
    {


        public string IsDeveloper
        {
            get { return (string)GetValue(IsDeveloperProperty); }
            set { SetValue(IsDeveloperProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDeveloper.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDeveloperProperty =
            DependencyProperty.Register("IsDeveloper", typeof(string), typeof(VnReleaseProducerModel), new PropertyMetadata(null));



        public string IsPublisher
        {
            get { return (string)GetValue(IsPublisherProperty); }
            set { SetValue(IsPublisherProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPublisher.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPublisherProperty =
            DependencyProperty.Register("IsPublisher", typeof(string), typeof(VnReleaseProducerModel), new PropertyMetadata(null));



        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(VnReleaseProducerModel), new PropertyMetadata(null));



        public string OriginalName
        {
            get { return (string)GetValue(OriginalNameProperty); }
            set { SetValue(OriginalNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OriginalName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginalNameProperty =
            DependencyProperty.Register("OriginalName", typeof(string), typeof(VnReleaseProducerModel), new PropertyMetadata(null));



        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(VnReleaseProducerModel), new PropertyMetadata(null));



    }
}
