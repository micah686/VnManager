using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VndbSharp;
using VndbSharp.Models;
using System.Diagnostics;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Drawing.FontFamily;
using Size = System.Drawing.Size;
using System.IO.Compression;
using VisualNovelManagerv2.ViewModel;
using VisualNovelManagerv2.ViewModel.Global;
using VndbSharp.Models.Common;
using VndbSharp.Models.Dumps;
using VndbSharp.Models.VisualNovel;

namespace VisualNovelManagerv2.Pages
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : UserControl
    {
        public Test()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            VndbSharp.Models.Common.SimpleDate sd = new SimpleDate();
            byte mon = 3;
            byte day = 7;
            sd.Month = 3;
            string test = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(13);
            Console.WriteLine(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mon));

            sd.Month = null;
            sd.Day = day;
            Console.WriteLine(sd.ToString());
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            //BitmapImage bi = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\error.png"));
            //StatusBarViewModel vm1 = (new ViewModelLocator()).StatusBar;
            //vm1.IsDbProcessing = true;
            //vm1.IsWorkProcessing = true;
            //vm1.ProgressStatus = bi;
            //vm1.ProgressBarStatus = 83.5;

            //vm1.ProgressPercentage = 55.3.ToString();
            //Globals.StatusBarViewModel.ProgressStatus = bi;

        }
    }
}
