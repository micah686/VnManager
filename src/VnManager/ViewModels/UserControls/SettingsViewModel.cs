using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using VndbSharp.Models.Common;
using System.Xml;
using System.Xml.Serialization;
using VnManager.Models.Settings;
using System.IO;

namespace VnManager.ViewModels.UserControls
{
    public class SettingsViewModel :Screen
    {
        public string Theme { get; set; } = "DarkTheme";
        public bool NsfwEnabled { get; set; }
        public bool NsfwContentSavedVisible { get; set; }
        public SpoilerLevel Spoiler { get; set; }



        public SettingsViewModel() { }

        public void SaveUserSettings()
        {
            UserSettingsVndb vndb = new UserSettingsVndb
            {
                Spoiler = Spoiler
            };
            UserSettings settings = new UserSettings
            {
                ColorTheme = Theme,
                IsNsfwEnabled = NsfwEnabled,
                IsVisibleSavedNsfwContent = NsfwContentSavedVisible,
                SettingsVndb = vndb
            };

            try
            {
                var serializer = new XmlSerializer(typeof(UserSettings));
                using (var writer = new StreamWriter(App.ConfigDirPath + @"\config\config.xml"))
                {
                    serializer.Serialize(writer, settings);
                }

                
            }
            catch (Exception ex)
            {
                
                throw;
            }
            


        }


    }

}
