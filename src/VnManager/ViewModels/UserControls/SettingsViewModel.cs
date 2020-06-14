﻿using Stylet;
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
using VnManager.ViewModels.Dialogs;

namespace VnManager.ViewModels.UserControls
{
    public class SettingsViewModel :Screen
    {
        public bool NsfwEnabled { get; set; }
        //public bool NsfwContentSavedVisible { get; set; }
        private bool _didChangeNsfwContentVisible = false;

        private bool _nsfwSavedContentVisible;
        public bool NsfwContentSavedVisible
        {
            get => _nsfwSavedContentVisible;
            set
            {
                _nsfwSavedContentVisible = value;
                SetAndNotify(ref _nsfwSavedContentVisible, value);
                _didChangeNsfwContentVisible = true;
            }
        }

        #region SpoilerList
        public Collection<string> SpoilerList { get;} = new Collection<string>(new string[] { App.ResMan.GetString("None"), App.ResMan.GetString("Minor"), App.ResMan.GetString("Major") });
        public string SpoilerString { get; set; }
        public int SpoilerIndex { get; set; } = 0;

        public Collection<string> ThemeList { get; } = new Collection<string>(new string[] { App.ResMan.GetString("DarkTheme"), App.ResMan.GetString("LightTheme") });
        public  int ThemeIndex { get; set; }
        public string ThemeString { get; set; } = "DarkTheme";
        #endregion


        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public SettingsViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            NsfwEnabled = App.UserSettings.IsNsfwEnabled;
            NsfwContentSavedVisible = App.UserSettings.IsVisibleSavedNsfwContent;
            if (App.UserSettings.SettingsVndb != null)
            {
                SpoilerString = App.UserSettings.SettingsVndb.Spoiler.ToString();

                switch (SpoilerString)
                {
                    case "None":
                        SpoilerIndex = 0;
                        break;
                    case "Minor":
                        SpoilerIndex = 1;
                        break;
                    case "Major":
                        SpoilerIndex = 2;
                        break;
                    default:
                        SpoilerIndex = 0;
                        break;
                }

                ThemeString = App.UserSettings.ColorTheme;
                switch (ThemeString)
                {
                    case "DarkTheme":
                        ThemeIndex = 0;
                        break;
                    case "LightTheme":
                        ThemeIndex = 1;
                        break;
                    default:
                        ThemeIndex = 0;
                        break;
                }
            }
            
        }

        public void SaveUserSettings(bool useEncryption)
        {
            Enum.TryParse(SpoilerString, out SpoilerLevel spoiler);
            string theme= String.Empty;
            switch (ThemeString)
            {
                case "Dark Theme":
                    theme = "DarkTheme";
                    break;
                case "Light Theme":
                    theme = "LightTheme";
                    break;
                default:
                    theme = "DarkTheme";
                    break;
            }
            
            UserSettingsVndb vndb = new UserSettingsVndb
            {
                Spoiler = spoiler
            };
            UserSettings settings = new UserSettings
            {
                ColorTheme = theme,
                IsNsfwEnabled = NsfwEnabled,
                IsVisibleSavedNsfwContent = NsfwContentSavedVisible,
                SettingsVndb = vndb,
                EncryptionEnabled = useEncryption
            };

            try
            {
                UserSettingsHelper.SaveUserSettings(settings);
                App.UserSettings = settings;

                if (_didChangeNsfwContentVisible == true)
                {
                    //add messagebox
                    DeleteNsfwImages();
                }
                UserSettingsHelper.UpdateColorTheme();
                
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
                List<VnInfoScreens> vnScreens = db.GetCollection<VnInfoScreens>("VnInfo_Screens").Query().Where(x => x.Nsfw == true).ToList();
                List<VnInfo> vnCovers = db.GetCollection<VnInfo>("VnInfo").Query().Where(x => x.ImageNsfw == true).ToList();

                ResetNsfwScreenshots(vnScreens);
                ResetNsfwCoverImages(vnCovers);
            }

        }

        private void ResetNsfwScreenshots(List<VnInfoScreens> vnScreens)
        {
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

                        Secure.EncFile(thumbFile);
                    }
                }
                else
                {
                    if (File.Exists($"{imageFile}.aes") && File.Exists($"{thumbFile}.aes"))
                    {
                        Secure.DecFile(imageFile);

                        Secure.DecFile(thumbFile);
                    }

                }
            }
        }

        private void ResetNsfwCoverImages(List<VnInfo> vnCovers)
        {
            foreach (var cover in vnCovers)
            {
                var directory = Path.Combine(App.AssetDirPath, @"sources\vndb\images\cover");
                var imageFile = $@"{directory}\{Path.GetFileName(cover.ImageLink)}";
                if (App.UserSettings.IsVisibleSavedNsfwContent == false)
                {
                    if (File.Exists(imageFile))
                    {
                        Secure.EncFile(imageFile);
                    }
                }
                else
                {
                    if (File.Exists(imageFile))
                    {
                        Secure.DecFile(imageFile);
                    }
                }

            }
        }




        public void ResetApplication()
        {
            var warning = _container.Get<DeleteEverythingViewModel>();
            bool? result = _windowManager.ShowDialog(warning);
            switch (result)
            {
                case null:
                    return;
                case true:
                {
                    if (App.AssetDirPath.Equals(App.ConfigDirPath))
                    {
                        Directory.Delete(App.AssetDirPath, true);
                    }
                    else
                    {
                        Directory.Delete(App.AssetDirPath, true);
                        Directory.Delete(App.ConfigDirPath, true);
                    }
                    CredentialManager.RemoveCredentials("VnManager.DbEnc");
                    CredentialManager.RemoveCredentials("VnManager.FileEnc");
                    Environment.Exit(0);
                    break;
                }
                default:
                    return;
            }
        }


    }

}
