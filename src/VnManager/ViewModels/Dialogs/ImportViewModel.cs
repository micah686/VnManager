// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using LiteDB.Engine;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Sentry;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs
{
    /// <summary>
    /// ViewModel for Importing an old database
    /// </summary>
    public class ImportViewModel: Screen
    {
        public BindableCollection<UserDataGames> UserDataGamesCollection { get; private set; } = new BindableCollection<UserDataGames>();
        public string DatabaseName { get; set; }

        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;
        private readonly ImportUserDataValidator _validator = new ImportUserDataValidator();

        public ImportViewModel(IWindowManager windowManager, IDialogService dialogService)
        {
            _windowManager = windowManager;
            _dialogService = dialogService;
        }


        /// <summary>
        /// Browse for the encrypted dump for import
        /// <see cref="BrowseImportDump"/>
        /// </summary>
        public void BrowseImportDump()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = $"{App.ResMan.GetString("BrowseDbDump")}",
                DefaultExt = ".vnbak",
                Filter = $"{App.ResMan.GetString("DbDump")} (*.vnbak)|*.vnbak",
                FileName = "VnManager_Export_YYYY-Month-DD.vnbak",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true,
                Multiselect = false
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                var filepath = settings.FileName;
                LoadDatabase(filepath);

            }
        }

        /// <summary>
        /// Loads the database, then fills the table with the UserData
        /// </summary>
        /// <param name="filePath">Path to the import database file</param>
        private void LoadDatabase(string filePath)
        {
            string dbError = App.ResMan.GetString("DbError");
            try
            {
                var didExpand = ImportExportHelper.Expand(filePath);
                if (!didExpand)
                {
                    return;
                }
                var dbPath = @$"{App.AssetDirPath}\Import.db";
                if (!File.Exists(dbPath))
                {
                    return;
                }
                using (var db = new LiteDatabase($"Filename={dbPath};Password={App.ImportExportDbKey}"))
                {
                    IEnumerable<UserDataGames> dbUserData =
                        db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).FindAll().ToArray();
                    var addList = new List<UserDataGames>();
                    foreach (var item in dbUserData)
                    {
                        var result = _validator.Validate(item);
                        foreach (var error in result.Errors)
                        {
                            item.SetError(error.PropertyName, error.ErrorMessage);
                        }

                        addList.Add(item);
                    }


                    UserDataGamesCollection.AddRange(addList);
                    File.Copy(@$"{App.AssetDirPath}\Import.db", Path.Combine(App.ConfigDirPath, @"database\Import.db"));
                    
                }

                DatabaseName = Path.GetFileName(filePath);
            }
            catch (IOException)
            {
                var errorStr = App.ResMan.GetString("DbIsLockedProc");
                _windowManager.ShowMessageBox(errorStr, dbError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to load database for import");
                SentryHelper.SendException(ex, null, SentryLevel.Error);
                _windowManager.ShowMessageBox($"{App.ResMan.GetString("ImportInvalidDb")}\n{Path.GetFileName(filePath)}",
                    $"{App.ResMan.GetString("ImportInvalidDbTitle")}", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        /// <summary>
        /// Dialog to select a new Exe file
        /// <see cref="BrowseExe"/>
        /// </summary>
        /// <param name="idCol">Column number from the dataTable</param>
        /// <returns></returns>
        public async Task BrowseExe(int idCol)
        {
            if (idCol != -1)
            {
                var settings = new OpenFileDialogSettings
                {
                    Title = $"{App.ResMan.GetString("BrowseForGame")}",
                    DefaultExt = ".exe",
                    Filter = $"{App.ResMan.GetString("Applications")} (*.exe)|*.exe",
                    FileName = "",
                    DereferenceLinks = true,
                    CheckPathExists = true,
                    CheckFileExists = true,
                    ValidateNames = true,
                    Multiselect = false
                };
                bool? result = _dialogService.ShowOpenFileDialog(this, settings);
                if (result == true)
                {
                    UserDataGamesCollection[idCol].ExePath = settings.FileName;
                    UserDataGamesCollection.Refresh();
                    UserDataGamesCollection[idCol].ClearAllErrors();
                    await _validator.ValidateAsync(UserDataGamesCollection[idCol]);
                }
            }

        }

        /// <summary>
        /// Dialog to select a new Icon file
        /// <see cref="BrowseIcon"/>
        /// </summary>
        /// <param name="idCol">Column number from the dataTable</param>
        /// <returns></returns>
        public async Task BrowseIcon(int idCol)
        {
            try
            {
                if (idCol != -1)
                {
                    var settings = new OpenFileDialogSettings
                    {
                        Title = $"{App.ResMan.GetString("BrowseForIcon")}",
                        DefaultExt = ".exe",
                        Filter = $"{App.ResMan.GetString("Icons")} (*.exe)|*.exe",
                        FileName = "",
                        DereferenceLinks = true,
                        CheckPathExists = true,
                        CheckFileExists = true,
                        ValidateNames = true
                    };
                    bool? result = _dialogService.ShowOpenFileDialog(this, settings);
                    if (result == true)
                    {
                        UserDataGamesCollection[idCol].IconPath = settings.FileName;
                        UserDataGamesCollection.Refresh();
                        UserDataGamesCollection[idCol].ClearAllErrors();
                        await _validator.ValidateAsync(UserDataGamesCollection[idCol]);
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to browse for icon");
                throw;
            }

        }

        /// <summary>
        /// Removes the selected row from the dataTable
        /// <see cref="RemoveRow"/>
        /// </summary>
        /// <param name="row">Column number from the dataTable</param>
        public void RemoveRow(int row)
        {
            try
            {
                UserDataGamesCollection.RemoveAt(row);
                UserDataGamesCollection.Refresh();
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to remove row from import tool");
            }


        }

        
        /// <summary>
        /// Checks data from import table, then updates the import db
        /// <see cref="ImportDataAsync"/>
        /// </summary>
        /// <returns></returns>
        public async Task ImportDataAsync()
        {
            try
            {
                var entryCount = UserDataGamesCollection.Count;
                var goodEntries = new List<UserDataGames>();

                if (entryCount < 1)
                {
                    return;
                }
                for (int i = 0; i < entryCount; i++)
                {

                    var entry = UserDataGamesCollection.First();
                    entry.ClearAllErrors();
                    var result = await _validator.ValidateAsync(entry);

                    if (result.Errors.Count > 0)
                    {
                        var error = result.Errors.First();
                        entry.SetError(error.PropertyName, error.ErrorMessage);
                        _windowManager.ShowMessageBox(App.ResMan.GetString("ValidationFailedRecheck"), App.ResMan.GetString("ValidationFailed"));
                        return;
                    }
                    
                    
                    var goodEntry = new UserDataGames()
                    {
                        Index = entry.Index, Categories = entry.Categories, Title = entry.Title,
                        SourceType = entry.SourceType, IconPath = entry.IconPath,
                        GameId = entry.GameId, Arguments = entry.Arguments, CoverPath = entry.CoverPath,
                        ExePath = entry.ExePath, ExeType = entry.ExeType, GameName = entry.GameName, Id = entry.Id,
                        LastPlayed = entry.LastPlayed, PlayTime = entry.PlayTime
                    };
                    goodEntries.Add(goodEntry);

                    

                    UserDataGamesCollection.RemoveAt(0);
                    UserDataGamesCollection.Refresh();

                }
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                var dbString = $"Filename={Path.Combine(App.ConfigDirPath, @"database\Import.db")};Password={App.ImportExportDbKey}";
                using (var db = new LiteDatabase(dbString))
                {
                    var userGames = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                    userGames.DeleteAll();
                    userGames.InsertBulk(goodEntries);
                    db.Rebuild(new RebuildOptions {Password = cred.Password});
                }
                
                File.Delete(Path.Combine(App.ConfigDirPath, App.DbPath));
                File.Move(Path.Combine(App.ConfigDirPath, @"database\Import.db"), Path.Combine(App.ConfigDirPath, App.DbPath));

                if (File.Exists(@$"{App.AssetDirPath}\Images.zip"))
                {
                    ZipFile.ExtractToDirectory(@$"{App.AssetDirPath}\Images.zip", @$"{App.AssetDirPath}\sources");
                    File.Delete(@$"{App.AssetDirPath}\Images.zip");
                }

                if (File.Exists(@$"{App.AssetDirPath}\Import.db"))
                {
                    File.Delete(@$"{App.AssetDirPath}\Import.db");
                }
                
                
                _windowManager.ShowMessageBox(App.ResMan.GetString("UserDataImported"), App.ResMan.GetString("ImportComplete"));
                RequestClose();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to import data");
                SentryHelper.SendException(ex, null, SentryLevel.Error);
                throw;
            }
            


        }

        /// <summary>
        /// Checks to see if the temporary Import.db still exists
        /// If it does, delete it
        /// </summary>
        protected override void OnClose()
        {
            if (File.Exists(Path.Combine(App.ConfigDirPath, @"database\Import.db")))
            {
                File.Delete(Path.Combine(App.ConfigDirPath, @"database\Import.db"));
            }
        }
    }

    
    public class ImportUserDataValidator : AbstractValidator<UserDataGames>
    {
        /// <summary>
        /// Validation methods
        /// </summary>
        public ImportUserDataValidator()
        {
            RuleFor(x => x.Id).Cascade(CascadeMode.Stop)
                .Must(IsValidGuid).WithMessage($"{App.ResMan.GetString("ValidationBadId")}")
                .Must(IsNotDuplicateGuid).WithMessage($"{App.ResMan.GetString("ValidationIdAlreadyExists")}");

            RuleFor(x => x.GameId).Cascade(CascadeMode.Stop)
                .Must(ValidateInteger).WithMessage($"{App.ResMan.GetString("ValidationBadGameID")}");

            RuleFor(x => x.SourceType).Must((x, y) => IsDefinedInEnum(x.SourceType, y.GetType()))
                .WithMessage($"{App.ResMan.GetString("ValidationBadSourceType")}");

            RuleFor(x => x.ExeType).Must((x, y) => IsDefinedInEnum(x.ExeType, y.GetType()))
                .WithMessage($"{App.ResMan.GetString("ValidationBadExeType")}");

            RuleFor(x => x.LastPlayed).Must(ValidateDateTime).WithMessage($"{App.ResMan.GetString("ValidationBadLastPlayed")}");

            RuleFor(x => x.PlayTime).Must(ValidateTimeSpan).WithMessage($"{App.ResMan.GetString("ValidationBadPlayTime")}");


            RuleFor(x => x.ExePath).Cascade(CascadeMode.Stop).ExeValidation();

            RuleFor(x => x.IconPath).Cascade(CascadeMode.Stop).IcoValidation().Unless(x =>
                string.IsNullOrEmpty(x.IconPath) || string.IsNullOrWhiteSpace(x.IconPath));

            RuleFor(x => x.Arguments).Cascade(CascadeMode.Stop).ArgsValidation().Unless(x =>
                string.IsNullOrEmpty(x.Arguments) || string.IsNullOrWhiteSpace(x.Arguments));

            RuleForEach(x => x.Categories).Must(ValidationHelpers.ContainsIllegalCharacters)
                .WithMessage(App.ResMan.GetString("ValidationArgumentsIllegalChars"));
        }

        #region Helpers
        /// <summary>
        /// Checks if Guid is valid
        /// </summary>
        /// <param name="guid">GUID to check</param>
        /// <returns></returns>
        public static bool IsValidGuid(Guid guid)
        {
            bool isValid = Guid.TryParse(guid.ToString(), out _);
            if (isValid && guid.Equals(Guid.Empty))
            {
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// Checks that there isn't a duplicate GUID
        /// </summary>
        /// <param name="id">GUID to check</param>
        /// <returns></returns>
        private static bool IsNotDuplicateGuid(Guid id)
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return false;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query().Select(x => x.Id).ToList();
                return !dbUserData.Contains(id);
            }
        }

        /// <summary>
        /// Checks if string is valid int
        /// </summary>
        /// <param name="id">id to validate</param>
        /// <returns></returns>
        private static bool ValidateInteger(Stringable<int> id)
        {
            var result = int.TryParse(id.StringValue, out _);
            return result;
        }

        /// <summary>
        /// Checks if the value is valid in the specified enum
        /// </summary>
        /// <param name="value">Enum to Validate</param>
        /// <param name="enumType">Specified Enum Type</param>
        /// <returns></returns>
        private static bool IsDefinedInEnum(Enum value, Type enumType)
        {
            if (value.GetType() != enumType)
            {
                return false;
            }

            return Enum.IsDefined(enumType, value);
        }

        /// <summary>
        /// checks if the DateTime is valid
        /// </summary>
        /// <param name="dateTime">DateTime to validate</param>
        /// <returns></returns>
        private static bool ValidateDateTime(DateTime dateTime)
        {
            var result = DateTime.TryParse(dateTime.ToString(CultureInfo.InvariantCulture), out _);
            return result;
        }

        /// <summary>
        /// Checks if the TimeSpan is valid
        /// </summary>
        /// <param name="ts">Timespan to validate</param>
        /// <returns></returns>
        private static bool ValidateTimeSpan(TimeSpan ts)
        {
            var result = TimeSpan.TryParse(ts.ToString(), CultureInfo.InvariantCulture, out _);
            return result;
        }
        #endregion
    }


}
