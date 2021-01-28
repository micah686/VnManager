// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;

namespace VnManager.Helpers
{
    public static class UserSettingsHelper
    {
        private static readonly string ConfigFile = Path.Combine(App.ConfigDirPath, @"config\config.json");


        /// <summary>
        /// Creates a default config file if one doesn't exist
        /// </summary>
        public static void CreateDefaultConfig()
        {
            if (!File.Exists(ConfigFile))
            {
                var settings = new UserSettings();
                UserSettingsVndb vndb = new UserSettingsVndb
                {
                    Spoiler = SpoilerLevel.None
                };
                settings.SettingsVndb = vndb;
                var json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(ConfigFile, json);
                App.UserSettings = settings;
            }

        }

        /// <summary>
        /// Validates that the config file is valid
        /// </summary>
        /// <returns></returns>
        public static bool ValidateConfigFile()
        {
            try
            {
                var input = File.ReadAllText(ConfigFile);
                JObject.Parse(input);
                return true;

            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to validate config file");
                return false;
            }
        }

        
        /// <summary>
        /// Reads the settings from the config file
        /// </summary>
        /// <returns>Returns the UserSettings</returns>
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

        /// <summary>
        /// Saves UserSettings over the config file
        /// </summary>
        /// <param name="settings">UserSettings with updated values</param>
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

        

    }
}
