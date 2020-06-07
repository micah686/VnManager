using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using VndbSharp.Models.Common;
using System.Xml;
using System.Xml.Serialization;
using VnManager.Models.Settings;
using System.IO;
using VnManager.Helpers;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace VnManager.ViewModels.UserControls
{
    public class SettingsViewModel :Screen
    {
        public string Theme { get; set; } = "DarkTheme";
        public bool NsfwEnabled { get; set; }
        public bool NsfwContentSavedVisible { get; set; }

        #region SpoilerList
        public Collection<string> SpoilerList { get;} = new Collection<string>(new string[] { App.ResMan.GetString("None"), App.ResMan.GetString("Minor"), App.ResMan.GetString("Major") });
        public string SpoilerString { get; set; }
        public int SpoilerIndex { get; set; } = 0;
        #endregion

        public static SettingsViewModel Instance { get; private set; }


        public SettingsViewModel()
        {
            if(Instance != null)
            {
                Instance = this;
            }            
        }

        public void SaveUserSettings(bool useEncryption = false)
        {
            Enum.TryParse(SpoilerString, out SpoilerLevel spoiler);
            UserSettingsVndb vndb = new UserSettingsVndb
            {
                Spoiler = spoiler
            };
            UserSettings settings = new UserSettings
            {
                ColorTheme = Theme,
                IsNsfwEnabled = NsfwEnabled,
                IsVisibleSavedNsfwContent = NsfwContentSavedVisible,
                SettingsVndb = vndb,
                EncryptionEnabled = useEncryption
            };

            try
            {
                UserSettingsHelper.SaveUserSettings(settings);
                App.UserSettings = settings;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't write to config file");
                throw;
            }
        }

        public void LoadUserSettings()
        {
            try
            {
                var settings = UserSettingsHelper.ReadUserSettings();
                App.UserSettings = settings;
                
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't load config file");
                throw;
            }
        }


        private void DeleteNsfwImages()
        {
            //Use CheckWriteAccess to see if you can delete from the images

            throw new NotImplementedException();
        }




    }

}
