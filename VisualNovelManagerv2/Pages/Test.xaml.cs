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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Thread.Sleep(150);
                stopwatch.Stop();
               
               
                VnUserData vnUserData;
                using (var context = new DatabaseContext())
                {
                    vnUserData = context.VnUserData.FirstOrDefault(x => x.VnId.Equals(Convert.ToUInt32(92)));
                    var lastPlayTime = vnUserData.LastPlayed.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> timecount = new List<int>();
                    for (int i = 0; i < lastPlayTime.Count(); i++)
                    {
                        timecount.Add(new int());
                        timecount[i] = Convert.ToInt32(lastPlayTime[i]);
                    }

                    TimeSpan timeSpan = new TimeSpan(timecount[0], timecount[1], timecount[2], timecount[3]);
                    TimeSpan currentplaytime = new TimeSpan(stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds);
                    timeSpan = timeSpan.Add(currentplaytime);
                }

                if (vnUserData != null)
                {
                    vnUserData.LastPlayed = DateTime.Now.ToString(CultureInfo.InvariantCulture);

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
