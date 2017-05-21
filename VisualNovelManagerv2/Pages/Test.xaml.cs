using System;
using System.Collections.Generic;
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
            using (Vndb client = new Vndb(false).WithClientDetails("VisualNovelManagerv2", "0.0.0"))
            {
                var ro = new RequestOptions();
                ro.sort = "id";
                ro.reverse = true;
                ro.count = 3;
                //var raw = await client.DoRawAsync("get vn basic (id>1) {\"sort\":\"id\",\"reverse\":true,\"results\":1}");
                var sample = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                client.Logout();
                Thread.Sleep(0);
            }
        }
    }
}
