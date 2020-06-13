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
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using VnManager.Models.Db.Vndb.Main;

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
                DeleteNsfwImages();
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


        public void DeleteNsfwImages()
        {
            //Use CheckWriteAccess to see if you can delete from the images
            var cred = CredentialManager.GetCredentials("VnManager.DbEnc");
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbVnInfo = db.GetCollection<VnInfoScreens>("VnInfo_Screens");
                var vnScreens = dbVnInfo.Query().Where(x => x.Nsfw == true).ToList();
                foreach (var screen in vnScreens)
                {
                    var directory = Path.Combine(App.AssetDirPath, @$"sources\vndb\images\screenshots\{screen.VnId}");
                    var imageFile = $@"{directory}\{Path.GetFileName(screen.ImageUrl)}";
                    var thumbFile = $@"{directory}\thumbs\{Path.GetFileName(screen.ImageUrl)}";

                    if (App.UserSettings.IsVisibleSavedNsfwContent == false)
                    {
                        if (File.Exists(imageFile) && File.Exists(thumbFile))
                        {
                            Secure.EncFile(imageFile);
                            File.Delete(imageFile);

                            Secure.EncFile(thumbFile);
                            File.Delete(thumbFile);
                        }
                        
                    }
                    else
                    {
                        if (File.Exists($"{imageFile}.aes") && File.Exists($"{thumbFile}.aes"))
                        {
                            Secure.DecFile(imageFile);
                            File.Delete($"{imageFile}.aes");

                            Secure.DecFile(thumbFile);
                            File.Delete($"{thumbFile}.aes");
                        }
                        
                    }



                }
            }


        }

        public void ResetApplication()
        {
            //add a messageBox with a 5 second delay and warning
            if (App.AssetDirPath.Equals(App.ConfigDirPath))
            {
                Directory.Delete(App.AssetDirPath, true);
            }
            else
            {
                Directory.Delete(App.AssetDirPath, true);
                Directory.Delete(App.ConfigDirPath, true);
            }
            Environment.Exit(0);
        }


    }

}
