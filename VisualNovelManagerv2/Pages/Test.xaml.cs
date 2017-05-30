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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //VnScreenshotViewModel.GetScreenshotList();
            VnMainViewModel.ClearCollectionsCommand.Execute(null);
            //test1();
            //Vndb vndb= new Vndb(true);
            //var vn = await vndb.GetVisualNovelAsync(VndbFilters.Id.Equals(92), VndbFlags.Tags);
            //var tags = vn.Items[0].Tags;
            //IEnumerable<Tag> tagDump = await VndbUtils.GetTagsDumpAsync();

            //foreach (var tag in tags)
            //{
            //    foreach (var tgTag in tagDump)
            //    {
            //        if (tgTag.Id == tag.Id)
            //        {
            //            Console.WriteLine($"dump id: {tgTag.Id}, tag to match:{tag.Id}\n");

            //        }
            //    }
            //}


            //Thread.Sleep(0);
        }

        private async void test1()
        {
            Vndb vndb = new Vndb(true);
            var vn = await vndb.GetVisualNovelAsync(VndbFilters.Id.Equals(92), VndbFlags.Tags);
            TagMetadata[] tags = vn.Items[0].Tags;
            IEnumerable<Tag> tagDump = await VndbUtils.GetTagsDumpAsync();

            var test = from tagMetadata in tags from tTag in tagDump where tTag.Id == tagMetadata.Id select tTag;

            
            foreach (var tag in tags)
            {
                foreach (var tgTag in tagDump)
                {
                    if (tgTag.Id == tag.Id)
                    {

                        Console.WriteLine(tgTag.Id);

                    }
                }
            };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VnMainViewModel.LoadBindVnDataCommand.Execute(null);
        }
    }
}
