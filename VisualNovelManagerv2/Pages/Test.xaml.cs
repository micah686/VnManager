using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
                VnUserData vnUserData;
                using (var context = new DatabaseContext())
                {
                    vnUserData = context.VnUserData.FirstOrDefault(x => x.VnId.Equals(Convert.ToUInt32(92)));
                }

                if (vnUserData != null)
                {
                    vnUserData.LastPlayed = DateTime.Now;
                    var test = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                    var dt = Convert.ToDateTime(test);
                }

                using (var context = new DatabaseContext())
                {
                    if (vnUserData != null)
                    {
                        context.Entry(vnUserData).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
