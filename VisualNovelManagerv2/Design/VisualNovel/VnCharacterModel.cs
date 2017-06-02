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
    public class VnCharacterModel: DependencyObject
    {

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));



        public string OriginalName
        {
            get { return (string)GetValue(OriginalNameProperty); }
            set { SetValue(OriginalNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OriginalName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginalNameProperty =
            DependencyProperty.Register("OriginalName", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));





        public string Gender
        {
            get { return (string)GetValue(GenderProperty); }
            set { SetValue(GenderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Gender.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GenderProperty =
            DependencyProperty.Register("Gender", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));





        public string BloodType
        {
            get { return (string)GetValue(BloodTypeProperty); }
            set { SetValue(BloodTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BloodType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BloodTypeProperty =
            DependencyProperty.Register("BloodType", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));



        public string Birthday
        {
            get { return (string)GetValue(BirthdayProperty); }
            set { SetValue(BirthdayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Birthday.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BirthdayProperty =
            DependencyProperty.Register("Birthday", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));



        public string Aliases
        {
            get { return (string)GetValue(AliasesProperty); }
            set { SetValue(AliasesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Aliases.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AliasesProperty =
            DependencyProperty.Register("Aliases", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));








        public FlowDocument Description
        {
            get { return (FlowDocument)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(FlowDocument), typeof(VnCharacterModel), new PropertyMetadata(null));





        public BitmapImage Image
        {
            get { return (BitmapImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(BitmapImage), typeof(VnCharacterModel), new PropertyMetadata(null));





        public string Bust
        {
            get { return (string)GetValue(BustProperty); }
            set { SetValue(BustProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bust.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BustProperty =
            DependencyProperty.Register("Bust", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));



        public string Waist
        {
            get { return (string)GetValue(WaistProperty); }
            set { SetValue(WaistProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Waist.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaistProperty =
            DependencyProperty.Register("Waist", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));



        public string Hip
        {
            get { return (string)GetValue(HipProperty); }
            set { SetValue(HipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HipProperty =
            DependencyProperty.Register("Hip", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));




        public string Height
        {
            get { return (string)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));




        public string Weigh
        {
            get { return (string)GetValue(WeighProperty); }
            set { SetValue(WeighProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Weigh.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WeighProperty =
            DependencyProperty.Register("Weigh", typeof(string), typeof(VnCharacterModel), new PropertyMetadata(null));




















    }
}
