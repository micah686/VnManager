﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Stylet;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbCharactersViewModel: Screen
    {
        
        public BitmapSource BackgroundImage { get; private set; }

        protected override void OnViewLoaded()
        {
            
            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                var filePath = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\4\8369.jpg";
                var uri = new Uri(filePath);
                BitmapSource bs = new BitmapImage(uri);
                BackgroundImage = bs;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                //throw;
            }

        }

        public static void ShowInfo()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnInfo();
        }

        public static void ShowScreens()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnScreenshots();
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            VndbContentViewModel.Instance.Cleanup();
        }

    }
}
