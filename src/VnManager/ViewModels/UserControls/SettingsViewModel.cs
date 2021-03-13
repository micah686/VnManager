// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using System;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;
using System.IO;
using VnManager.Helpers;
using AdysTech.CredentialManager;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using Sentry;
using VnManager.ViewModels.Dialogs;

namespace VnManager.ViewModels.UserControls
{
    public class SettingsViewModel :Screen
    {

        
        public int SpoilerIndex { get; set; } = 0;
        public int MaxSexualIndex { get; set; }
        public int MaxViolenceIndex { get; set; }


        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly Func<DeleteEverythingViewModel> _deleteEverythingFactory;
        /// <summary>
        /// Main instance of the Settings View
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="dialogService"></param>
        /// <param name="deleteEverything"></param>
        public SettingsViewModel(IWindowManager windowManager, IDialogService dialogService, Func<DeleteEverythingViewModel> deleteEverything)
        {
            _windowManager = windowManager;
            _dialogService = dialogService;
            _deleteEverythingFactory = deleteEverything;
            FillSexualDropdown();
            FillViolenceDropdown();
            FillSpoilerDropdown();
        }

        /// <summary>
        /// Fill the Sexual dropdown list with available values
        /// </summary>
        private void FillSexualDropdown()
        {
            try
            {
                if (App.UserSettings.SettingsVndb != null)
                {
                    switch (App.UserSettings.MaxSexualRating)
                    {
                        case SexualRating.Safe:
                            MaxSexualIndex = (int)SexualRating.Safe;
                            break;
                        case SexualRating.Suggestive:
                            MaxSexualIndex = (int)SexualRating.Suggestive;
                            break;
                        case SexualRating.Explicit:
                            MaxSexualIndex = (int)SexualRating.Explicit;
                            break;
                        default:
                            MaxSexualIndex = 0;
                            _windowManager.ShowMessageBox(App.ResMan.GetString("UserSettingsInvalid"), App.ResMan.GetString("Error"));
                            App.Logger.Warning("UserSetting Sexual Rating is invalid");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to fill Sexual dropdown on Settings");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }
        }

        /// <summary>
        /// Fill the Violence dropdown list with available values
        /// </summary>
        private void FillViolenceDropdown()
        {
            try
            {
                if (App.UserSettings.SettingsVndb != null)
                {
                    switch (App.UserSettings.MaxViolenceRating)
                    {
                        case ViolenceRating.Tame:
                            MaxViolenceIndex = (int)ViolenceRating.Tame;
                            break;
                        case ViolenceRating.Violent:
                            MaxViolenceIndex = (int)ViolenceRating.Violent;
                            break;
                        case ViolenceRating.Brutal:
                            MaxViolenceIndex = (int)ViolenceRating.Brutal;
                            break;
                        default:
                            MaxViolenceIndex = 0;
                            _windowManager.ShowMessageBox(App.ResMan.GetString("UserSettingsInvalid"), App.ResMan.GetString("Error"));
                            App.Logger.Warning("UserSetting Violence Rating is invalid");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to fill Violence dropdown on Settings");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }
        }

        /// <summary>
        /// Fill the Spoiler dropdown list with available values
        /// </summary>
        private void FillSpoilerDropdown()
        {
            try
            {
                if (App.UserSettings.SettingsVndb != null)
                {
                    switch (App.UserSettings.SettingsVndb.Spoiler)
                    {
                        case SpoilerLevel.None:
                            SpoilerIndex = (int)SpoilerLevel.None;
                            break;
                        case SpoilerLevel.Minor:
                            SpoilerIndex = (int)SpoilerLevel.Minor;
                            break;
                        case SpoilerLevel.Major:
                            SpoilerIndex = (int)SpoilerLevel.Major;
                            break;
                        default:
                            SpoilerIndex = 0;
                            _windowManager.ShowMessageBox(App.ResMan.GetString("UserSettingsInvalid"), App.ResMan.GetString("Error"));
                            App.Logger.Warning("UserSetting Spoiler Level is invalid");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to fill Spoiler dropdown on Settings");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }
        }

        /// <summary>
        /// Save the currently selected settings
        /// </summary>
        /// <param name="useEncryption"></param>
        public void SaveUserSettings(bool useEncryption)
        {

            UserSettingsVndb vndb = new UserSettingsVndb
            {
                Spoiler = (SpoilerLevel)SpoilerIndex
            };
            UserSettings settings = new UserSettings
            {
                SettingsVndb = vndb,
                RequirePasswordEntry = useEncryption,
                MaxSexualRating = (SexualRating)MaxSexualIndex,
                MaxViolenceRating = (ViolenceRating)MaxViolenceIndex
            };

            try
            {
                UserSettingsHelper.SaveUserSettings(settings);
                App.UserSettings = settings;

                _windowManager.ShowMessageBox(App.ResMan.GetString("SettingsSavedMessage"), App.ResMan.GetString("SettingsSavedTitle"));
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't write to config file");
                SentryHelper.SendException(ex, null, SentryLevel.Error);
                throw;
            }
        }

        //TODO: Is this being used?
        public static void LoadUserSettings()
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

        /// <summary>
        /// Export the Userdata and images into the .vnbak file
        /// <see cref="ExportUserData"/>
        /// </summary>
        public void ExportUserData()
        {
            try
            {
                string savePath;
                var settings = new FolderBrowserDialogSettings();
                bool? result = _dialogService.ShowFolderBrowserDialog(this, settings);
                if (result == true)
                {
                    savePath = settings.SelectedPath;
                }
                else
                {
                    return;
                }

            
                var fileName = $@"{savePath}\VnManager_Export_{DateTime.UtcNow:yyyy-MMMM-dd}.vnbak";
                var didCreate = ImportExportHelper.Compact(fileName);
                if (didCreate)
                {
                    _windowManager.ShowMessageBox($"{App.ResMan.GetString("UserDataExportedPath")}\n{fileName}", $"{App.ResMan.GetString("UserDataExportedTitle")}");
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to export userData");
                SentryHelper.SendException(e, null, SentryLevel.Error);
            }
        }

        /// <summary>
        /// Clear out ALL data and config created by the application
        /// <see cref="ResetApplication"/>
        /// </summary>
        public void ResetApplication()
        {
            try
            {
                var warning = _deleteEverythingFactory();
                bool? result = _windowManager.ShowDialog(warning);
                switch (result)
                {
                    case null:
                        return;
                    case true:
                    {
                        DeleteEverything();
                        break;
                    }
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to reset application");
                SentryHelper.SendException(e, null, SentryLevel.Error);
            }
        }

        /// <summary>
        /// Delete all files associated with the application, then exit
        /// </summary>
        private void DeleteEverything()
        {
            try
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
                CredentialManager.RemoveCredentials(App.CredDb);
                CredentialManager.RemoveCredentials(App.CredFile);
                _windowManager.ShowMessageBox($"{App.ResMan.GetString("AppExit")}");
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to delete all VnManager files");
                SentryHelper.SendException(e, null, SentryLevel.Error);
            }
        }

    }

}
