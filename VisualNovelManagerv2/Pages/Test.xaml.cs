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
                    

                    #region AddM2M

                    //Category category = new Category
                    //{
                    //    CategoryName = "New Category"
                    //};

                    //VnUserCategoryTitle uc = new VnUserCategoryTitle { VnId = 4, Title = "Vn01" };
                    //VnUserCategoryTitle uc2 = new VnUserCategoryTitle { VnId = 5, Title = "Vn02" };
                    //VnUserCategoryTitle uc3 = new VnUserCategoryTitle { VnId = 46, Title = "Vn03" };

                    //context.CategoryJunction.Add(new CategoryJunction { Category = category, VnUserCategoryTitle = uc });
                    //context.CategoryJunction.Add(new CategoryJunction { Category = category, VnUserCategoryTitle = uc2 });
                    //context.CategoryJunction.Add(new CategoryJunction { Category = category, VnUserCategoryTitle = uc3 });
                    //context.SaveChanges();

                    #endregion

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
