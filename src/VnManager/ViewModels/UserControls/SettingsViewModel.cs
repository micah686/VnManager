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
using VnManager.Helpers;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace VnManager.ViewModels.UserControls
{
    public class SettingsViewModel :Screen
    {
        private static readonly ResourceManager Rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
        private const string configFile = @"\config\config.xml";
        public string Theme { get; set; } = "DarkTheme";
        public bool NsfwEnabled { get; set; }
        public bool NsfwContentSavedVisible { get; set; }

        #region SpoilerList
        public List<string> SpoilerList { get; set; } = new List<string>(new string[] { Rm.GetString("None"), Rm.GetString("Minor"), Rm.GetString("Major") });
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

        public void SaveUserSettings()
        {
            SpoilerLevel spoiler;
            Enum.TryParse(SpoilerString, out spoiler);
            UserSettingsVndb vndb = new UserSettingsVndb
            {
                Spoiler = spoiler
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
                using (var writer = new StreamWriter(App.ConfigDirPath + configFile))
                {
                    serializer.Serialize(writer, settings);
                }
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
            if(!File.Exists(App.ConfigDirPath + configFile))
            {
                CreateDefaultConfig();
            }
            try
            {
                var serializer = new XmlSerializer(typeof(UserSettings));
                using (var fs = new FileStream(App.ConfigDirPath + configFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool isValid = ValidateXml.IsValidXml(App.ConfigDirPath + configFile);
                    App.UserSettings = isValid == true ? (UserSettings)serializer.Deserialize(fs) : null;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't load config file");
                throw;
            }
        }

        public static void LoadUserSettingsStatic()
        {
            if(Instance== null)
            {
                Instance = new SettingsViewModel();
            }
            Instance.LoadUserSettings();
        }


        private void CreateDefaultConfig()
        {
            var settings = new UserSettings();
            try
            {
                var serializer = new XmlSerializer(typeof(UserSettings));
                using (var writer = new StreamWriter(App.ConfigDirPath + configFile))
                {
                    serializer.Serialize(writer, settings);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't write to config file");
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
