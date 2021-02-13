// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Stylet;

namespace VnManager.ViewModels.Dialogs
{
    public class AboutViewModel: Screen
    {
        public string Title { get; set; }
        public string SoftwareVersion { get; private set; }
        public string CopyrightDate { get; private set; }
        public string Website { get; private set; }
        public string LicenseInfo { get; private set; }
        public string DeveloperName { get; private set; }

        public AboutViewModel()
        {
            PopulateData();
        }

        private void PopulateData()
        {
            Title = App.ResMan.GetString("About");
            SoftwareVersion = $"VnManager {GetVersionInfo()}";
            CopyrightDate = $"{App.ResMan.GetString("Copyright")} 2021-{DateTime.UtcNow.Year}";
            Website = @"https://github.com/micah686/VnManager";
            LicenseInfo = App.ResMan.GetString("LicensedUnderMIT");
            DeveloperName = $"{App.ResMan.GetString("DevelopedBy")} Micah686";
        }
        
        private string GetVersionInfo()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            return version;
        }

        public void WebsiteClick()
        {
            var link = Website;
            var ps = new ProcessStartInfo(link)
            {
                UseShellExecute = true
            };
            Process.Start(ps);
        }

        public void CloseClick()
        {
            RequestClose();
        }
    }
}
