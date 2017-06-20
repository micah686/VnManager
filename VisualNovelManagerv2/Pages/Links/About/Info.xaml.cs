using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualNovelManagerv2.Pages.Links.About
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : UserControl
    {
        public Info()
        {
            InitializeComponent();
            Github.Source = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\assorted\github.png"));
            CSharp.Source = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\assorted\csharp.png"));
            Xaml.Source = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\assorted\xaml.png"));
            VisualStudio.Source = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\assorted\visualstudio.png"));
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
