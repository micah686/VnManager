// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System.ComponentModel;
using Stylet;

namespace VnManager.Helpers
{
    public static class FileDialogHelper
    {
        private static readonly OpenFileDialogSettings DefaultOpenFileDialogSettings = new OpenFileDialogSettings
        {
            FileName = "",
            DereferenceLinks = true,
            CheckPathExists = true,
            CheckFileExists = true,
            ValidateNames = true,
            Multiselect = false
        };

        public static string BrowseExe(IDialogService dialogService, object ownerVm)
        {
            var settings = DefaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseExe") ?? string.Empty;
            settings.DefaultExt = ".exe";
            settings.Filter = "Applications (*.exe)|*.exe";

            if (dialogService != null && ownerVm != null)
            {
                bool? result = dialogService.ShowOpenFileDialog((INotifyPropertyChanged)ownerVm, settings);
                if (result == true)
                {
                    var exePath = settings.FileName;
                    return exePath;
                }

                return string.Empty;
            }

            return string.Empty;
        }

        public static string BrowseIcon(IDialogService dialogService, object ownerVm)
        {
            var settings = DefaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseForIcon") ?? string.Empty;
            settings.DefaultExt = ".ico";
            settings.Filter = "Icons (*.ico,*.exe)|*.ico;*.exe";

            if (dialogService != null && ownerVm != null)
            {
                bool? result = dialogService.ShowOpenFileDialog((INotifyPropertyChanged)ownerVm, settings);
                if (result == true)
                {
                    var exePath = settings.FileName;
                    return exePath;
                }

                return string.Empty;
            }

            return string.Empty;
        }

        public static string BrowseCover(IDialogService dialogService, IWindowManager windowManager, object ownerVm)
        {
            var settings = DefaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseForCover") ?? string.Empty;
            settings.DefaultExt = ".jpg";
            settings.Filter = "Images (*.jpg,*.png)|*.jpg;*.png";

            if (dialogService != null && windowManager!= null && ownerVm != null)
            {
                bool? result = dialogService.ShowOpenFileDialog((INotifyPropertyChanged)ownerVm, settings);
                if (result == true)
                {
                    if (ImageHelper.IsValidImage(settings.FileName))
                    {
                        var coverPath = settings.FileName;
                        return coverPath;
                    }

                    windowManager.ShowMessageBox(App.ResMan.GetString("ValidationNotValidImage"));
                    return string.Empty;
                }

                return string.Empty;
            }

            return string.Empty;
        }
    }
}
