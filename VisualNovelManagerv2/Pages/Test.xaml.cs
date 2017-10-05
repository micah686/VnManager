using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Design.Settings;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VndbSharp;
using VndbSharp.Models;
#pragma warning disable 1998


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
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.IsDownloading = true;
            Globals.StatusBar.IsUploading = true;
            Globals.StatusBar.ProgressPercentage = 30.5;
            Globals.StatusBar.ProgressText = "testing123";
            Globals.StatusBar.Message = "msgtest";
            Globals.StatusBar.IsShowOnlineStatusEnabled = true;
            Globals.StatusBar.SetOnlineStatusColor(true);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            UserSettings userSettings = new UserSettings
            {
                NsfwEnabled = false,
                MaxSpoilerLevel = 2,
                VnSetting = new VnSetting
                {
                    Id = 11,
                    Spoiler = 3
                }
            };
            ModifyUserSettings.SaveUserSettings(userSettings);


            var foo = ModifyUserSettings.LoadUserSettings();
            var test = foo.ToString();

            ModifyUserSettings.RemoveUserSettingsNode(11);
        }


        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    byte[] foo = new byte[2];
                    var bar = foo[999];
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Globals.Logger.Error(exception);
                throw;
            }
        }
    }
}
