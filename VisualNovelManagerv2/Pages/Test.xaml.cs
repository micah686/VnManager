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
using System.IO;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Drawing.FontFamily;
using Size = System.Drawing.Size;
using System.IO.Compression;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Design.Settings;
using VisualNovelManagerv2.EntityFramework;
using VisualNovelManagerv2.EntityFramework.Entity.VnInfo;
using VisualNovelManagerv2.EntityFramework.Entity.VnOther;
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
            UserSettings userSettings = new UserSettings();
            userSettings.NsfwEnabled = false;
            userSettings.MaxSpoilerLevel = 2;
            //userSettings.VnSetting = new VnSetting
            //{
            //    Id = 11,
            //    Spoiler = 3
            //};
            ModifyUserSettings.SaveUserSettings(userSettings);


            var foo = ModifyUserSettings.LoadUserSettings();
            var test = foo.ToString();

            ModifyUserSettings.RemoveUserSettingsNode(11);
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
