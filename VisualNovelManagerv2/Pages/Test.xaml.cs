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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VnScreenshotViewModel.GetScreenshotList();
        }

        

        private float Foo(string text)
        {
            Font stringFont = new Font("Segoe UI", 13, System.Drawing.FontStyle.Regular);
            Bitmap b = new Bitmap(2200, 2200);
            Graphics g = Graphics.FromImage(b);
            SizeF sizeOfString = new SizeF();
            sizeOfString = g.MeasureString(text, stringFont);
            return sizeOfString.Width;
        }
    }
}
