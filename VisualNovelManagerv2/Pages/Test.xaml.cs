using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Design.Settings;
using VisualNovelManagerv2.EF.Data;
using VisualNovelManagerv2.EF.Data.Context;
using VisualNovelManagerv2.EF.Data.Entity.VnOther;

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
            userSettings.VnSetting = new VnSetting
            {
                Id = 11,
                Spoiler = 3
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


            var test = new DatabaseContext();

            using (var db = new DatabaseContext())
            {
              VisualNovelManagerv2.EF.Data.Entity.VnOther.Categories categories = new Categories()
              {
                  Category = "testcat"
              };
                db.Add(categories);
                db.SaveChanges();

                foreach (var cats in db.Set<Categories>())
                {
                    Console.WriteLine(cats.Category);
                }
            }
        }
    }
}
