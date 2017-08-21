using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Design.Settings;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;


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


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var regex = new Regex(@"^(10|[1-9]{1,2}){1}(\.[0-9]{1,2})?$");
            var result = regex.Match(1.3.ToString());
            var sample = "3.4";
            byte test1 = Convert.ToByte(sample.Replace(".", string.Empty));


            

            using (var db = new DatabaseContext())
            {
                try
                {
                    

                    VnInfoLinks vnInfoLinks = new VnInfoLinks()
                    {
                        Wikipedia = "wiki01"
                    };
                    db.VnInfoLinks.Add(vnInfoLinks);
                    db.SaveChanges();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
                

            }
        }
    }
}
