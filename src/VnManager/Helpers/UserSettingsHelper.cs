using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using AdonisUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;

namespace VnManager.Helpers
{
    public static class UserSettingsHelper
    {
        private static readonly string ConfigFile = Path.Combine(App.ConfigDirPath, @"config\config.json");


        public static void CreateDefaultConfig()
        {
            if (!File.Exists(ConfigFile))
            {
                var settings = new UserSettings();
                UserSettingsVndb vndb = new UserSettingsVndb
                {
                    Spoiler = SpoilerLevel.None
                };
                settings.ColorTheme = "DarkTheme";
                settings.SettingsVndb = vndb;
                var json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(ConfigFile, json);
                App.UserSettings = settings;
            }

        }

        public static bool ValidateConfigFile()
        {
            try
            {
                var input = File.ReadAllText(ConfigFile);
                var output = JObject.Parse(input);
                return true;

            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to validate config file");
                return false;
            }
        }

        

        public static UserSettings ReadUserSettings()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var isValid = ValidateConfigFile();
                    if (isValid)
                    {
                        var json = File.ReadAllText(ConfigFile);
                        var settings = JsonConvert.DeserializeObject<UserSettings>(json);
                        return settings;
                    }
                    File.Delete(ConfigFile);
                    CreateDefaultConfig();
                    var output = File.ReadAllText(ConfigFile);
                    var userSettings = JsonConvert.DeserializeObject<UserSettings>(output);
                    return userSettings;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't read user settings");
                return null;
            }
        }


        public static void SaveUserSettings(UserSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to save user settings");
                throw;
            }
        }

        public static void UpdateColorTheme()
        {
            if(App.UserSettings == null) return;
            var theme = App.UserSettings.ColorTheme;
            ResourceLocator.SetColorScheme(Application.Current.Resources,
                theme == "LightTheme" ? ResourceLocator.LightColorScheme : ResourceLocator.DarkColorScheme);
        }

    }
}
