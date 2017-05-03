using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Licenses.xaml
    /// </summary>
    public partial class Licenses : UserControl
    {
        public Licenses()
        {
            InitializeComponent();
            ObservableCollection<LicenseData> data = GetData();

            //Bind the DataGrid to the LicenseData data
            LicenseDataGrid.DataContext = data;
        }


        private ObservableCollection<LicenseData> GetData()
        {
            var data = new ObservableCollection<LicenseData>();
            data.Add(new LicenseData { Software = "MVVMLight", Info = "Toolkit for MVVM in WPF", Url = "https://mvvmlight.codeplex.com", License = "MIT",  });
            data.Add(new LicenseData { Software = "MUI", Info = "ModernUI for WPF(MUI)", Url = "https://github.com/firstfloorsoftware/mui", License = "MS-PL",  });
            data.Add(new LicenseData { Software = "SQLite", Info = "SQLite database engine", Url = "https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki", License = "MS-PL",  });
            data.Add(new LicenseData { Software = "System.Net.Security", Info = "Provides types for SSL/TLS", Url = "https://www.nuget.org/packages/System.Net.Security/", License = "Microsoft Proprietary"});
            data.Add(new LicenseData { Software = "System.Net.Sockets", Info = "Provides types for sockets", Url = "https://www.nuget.org/packages/System.Net.Sockets", License = "Microsoft Proprietary" });
            data.Add(new LicenseData { Software = "System.Reflection.TypeExtensions", Info = "Provides extensions for reflection", Url = "https://www.nuget.org/packages/System.Reflection.TypeExtensions", License = "Microsoft Proprietary" });
            data.Add(new LicenseData { Software = "System.Security.SecureString", Info = "Provides the SecureString class", Url = "https://www.nuget.org/packages/System.Security.SecureString", License = "Microsoft Proprietary" });
            data.Add(new LicenseData { Software = "MvvmValidation", Info = "Framework that removes boilerplate for validation", Url = "https://www.nuget.org/packages/MvvmValidation", License = "MIT" });
            return data;
        }
    }

    public class LicenseData
    {
        public string Software { get; set; }
        public string Info { get; set; }
        public string Url { get; set; }
        public string License { get; set; }
    }
}
