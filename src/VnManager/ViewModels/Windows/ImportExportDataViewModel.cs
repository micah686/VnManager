using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using Stylet;
using StyletIoC;
using VndbSharp.Models;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Windows
{
    public class ImportExportDataViewModel: Screen
    {
        public BindableCollection<UserDataGames> UserDataGamesCollection { get; set; }
        public bool IsDataGridEnabled { get; set; } = false;
        private bool _didCancelImport = false;
        public bool IsImportingVisible { get; set; } = false;

        public string ImportMessage { get; set; }
        public string CurrentProgressMsg { get; set; }
        public string TotalProgressMsg { get; set; }
        public double TotalProgress { get; set; }
        public bool IsImportProcessing { get; set; }
        
        
        
        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;
        public ImportExportDataViewModel(IWindowManager windowManager, IDialogService dialogService, IModelValidator<ImportExportDataViewModel> validator):base(validator)
        {
            _windowManager = windowManager;
            _dialogService = dialogService;
            UserDataGamesCollection = new BindableCollection<UserDataGames>();
        }

        public void ExportData()
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
            var fileName = $@"{savePath}\VnManager_Export_{DateTime.UtcNow:yyyy-MMMM-dd}.db";
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").FindAll();

                using (var exportDatabase = new LiteDatabase(fileName))
                {
                    var exportUserData = exportDatabase.GetCollection<UserDataGames>("UserData_Games");
                    
                    List<UserDataGames> userDataList = dbUserData.Select(item => new UserDataGames
                        {
                            Id = item.Id,
                            GameId = item.GameId,
                            GameName = item.GameName,
                            SourceType = item.SourceType,
                            LastPlayed = item.LastPlayed,
                            PlayTime = item.PlayTime,
                            Categories = item.Categories,
                            ExePath = item.ExePath,
                            IconPath = item.IconPath,
                            Arguments = item.Arguments
                        })
                        .ToList();

                    exportUserData.Insert(userDataList);
                    _windowManager.ShowMessageBox($"{App.ResMan.GetString("UserDataExportedPath")}\n{fileName}", $"{App.ResMan.GetString("UserDataExportedTitle")}");

                }


            }
        }



        public void BrowseImportDump()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = $"{App.ResMan.GetString("BrowseDbDump")}",
                DefaultExt = ".db",
                Filter = $"{App.ResMan.GetString("DbDump")} (*.db)|*.db",
                FileName = "VnManager_Export_YYYY-Month-DD.db",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                var filepath = settings.FileName;
                LoadDatabase(filepath);

            }
        }

        private void LoadDatabase(string filePath)
        {
            try
            {
                using (var db = new LiteDatabase($"{filePath}"))
                {
                    IEnumerable<UserDataGames> dbUserData =
                        db.GetCollection<UserDataGames>("UserData_Games").FindAll().ToArray();
                    var addList = new List<UserDataGames>();
                    var validator = new ImportUserDataValidator();
                    foreach (var item in dbUserData)
                    {
                        var result = validator.Validate(item);
                        foreach (var error in result.Errors)
                        {
                            item.SetError(error.PropertyName, error.ErrorMessage);
                        }
                        addList.Add(item);
                    }


                    UserDataGamesCollection.AddRange(addList);
                    IsDataGridEnabled = true;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to load database for import");
                _windowManager.ShowMessageBox($"{App.ResMan.GetString("ImportInvalidDb")}\n{Path.GetFileName(filePath)}",
                    $"{App.ResMan.GetString("ImportInvalidDbTitle")}", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void BrowseExe(int idCol)
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
                    ValidateNames = true
                };
                bool? result = _dialogService.ShowOpenFileDialog(this, settings);
                if (result == true)
                {
                    UserDataGamesCollection[idCol].ExePath = settings.FileName;
                    UserDataGamesCollection.Refresh();
                }
            }
            
        }

        public void BrowseIcon(int idCol)
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
                }
            }
                
        }

        public void RemoveRow(int row)
        {
            try
            {
                UserDataGamesCollection.RemoveAt(row);
                UserDataGamesCollection.Refresh();
            }
            catch (Exception e)
            {
                App.Logger.Warning(e,"Failed to remove row from import tool");
                //throw;
            }
            
            
        }

        public async Task ValidateData()
        {
            int errorCount = 0;
            var validator = new ImportUserDataValidator();
            foreach (var item in UserDataGamesCollection)
            {
                var result = await validator.ValidateAsync(item);
                foreach (var error in result.Errors)
                {
                    errorCount += 1;
                    item.SetError(error.PropertyName, error.ErrorMessage);
                }
                
            }

            if (errorCount > 0)
            {
                _windowManager.ShowMessageBox($"{App.ResMan.GetString("ValidationFailedRecheck")}");
            }
            else
            {
                await ImportData();
            }
        }

        private async Task ImportData()
        {
            try
            {
                var dbPath = Path.Combine(App.ConfigDirPath, @"database\Data.db");
                var now = $"{DateTime.Now:yyyy-MM-dd}__{DateTime.Now:h-mmtt}";
                File.Copy(dbPath, Path.Combine(App.ConfigDirPath, $@"database\Data_BACKUP_{now}.db"));


                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return;
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    var dbUserData = db.GetCollection<UserDataGames>("UserData_Games");
                    var maxId = dbUserData.Query().OrderByDescending(x => x.Index).Select(x => x.Index)
                        .FirstOrDefault();
                    var importList = new List<UserDataGames>();
                    foreach (var item in UserDataGamesCollection)
                    {
                        maxId = maxId + 1;
                        item.Index = maxId;
                        importList.Add(item);
                    }
                    UserDataGamesCollection.Clear();
                    UserDataGamesCollection.AddRange(importList);
                    await Task.Run(() =>
                    {

                        dbUserData.Insert(UserDataGamesCollection);
                    });


                    Stringable<int>[] vndbIds = UserDataGamesCollection.Where(x => x.SourceType == AddGameSourceType.Vndb)
                        .Select(x => x.GameId).ToArray();
                    await UpdateVndbData(vndbIds);
                }

            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to import data");
                throw;
            }
        }

        private async Task UpdateVndbData(Stringable<int>[] gameIds)
        {
            RequestOptions ro = new RequestOptions(){Count = 25};
            var getData = new MetadataProviders.Vndb.GetVndbData();
            var saveData = new MetadataProviders.Vndb.SaveVnDataToDb();
            using (var client = new VndbSharp.Vndb(true))
            {
                IsImportingVisible = true;
                IsImportProcessing = true;
                ImportMessage = $"{App.ResMan.GetString("ImportDbStarting")}";
                TotalProgressMsg = $"{App.ResMan.GetString("TotalProgressColon")} 0/{gameIds.Length}";
                var totalIncrement = 100 / gameIds.Length;
                var count = 0;


                foreach (var strId in gameIds)
                {
                    if (_didCancelImport) break;
                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg1")}";
                    var id = (uint)strId.Value;
                    var visualNovel = await getData.GetVisualNovel(client, id);
                    var releases = await getData.GetReleases(client, id, ro);
                    uint[] producerIds = releases.SelectMany(x => x.Producers.Select(y => y.Id)).Distinct().ToArray();
                    var producers = await getData.GetProducers(client, producerIds, ro);
                    var characters = await getData.GetCharacters(client, id, ro);
                    uint[] staffIds = visualNovel.Staff.Select(x => x.StaffId).Distinct().ToArray();
                    var staff = await getData.GetStaff(client, staffIds, ro);

                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg2")}";
                    saveData.SaveVnInfo(visualNovel);
                    saveData.SaveVnCharacters(characters, id);
                    saveData.SaveVnReleases(releases);
                    saveData.SaveProducers(producers);
                    saveData.SaveStaff(staff, (int)id);

                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg3")}";
                    await DownloadVndbContent.DownloadCoverImage(id);
                    await DownloadVndbContent.DownloadCharacterImages(id);
                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg4")}";
                    await DownloadVndbContent.DownloadScreenshots(id);


                    TotalProgress += totalIncrement;
                    count = count + 1;
                    TotalProgressMsg = $"{App.ResMan.GetString("TotalProgressColon")} {count}/ {gameIds.Length}";
                }

                if (_didCancelImport)
                {
                    ImportMessage = $"{App.ResMan.GetString("ImportDbCancel")}";
                }
                else
                {
                    ImportMessage = $"{App.ResMan.GetString("ImportComplete")}. \n {App.ResMan.GetString("CanCloseWindow")}";
                }

                CurrentProgressMsg = $"{ App.ResMan.GetString("Done")}";
                IsImportProcessing = false;

            }
            _windowManager.ShowMessageBox($"{App.ResMan.GetString("UserDataImported")}");
        }



        public void CancelImport()
        {
            _didCancelImport = true;
            ImportMessage = $"{App.ResMan.GetString("ImportDbCancelWorking")}";
        }

    }

    public class ImportExportDataViewModelValidator : AbstractValidator<ImportExportDataViewModel>
    {
        public ImportExportDataViewModelValidator()
        {
            //RuleForEach(x => x.UserDataGamesCollection).SetValidator(new ImportUserDataValidator());
            RuleFor(x => x.UserDataGamesCollection[9].ExePath)
                .NotEmpty().WithMessage("empty")
                .NotEqual("BAD").WithMessage("bad value");
        }
    }

    public class ImportUserDataValidator : AbstractValidator<UserDataGames>
    {
        public ImportUserDataValidator()
        {
            RuleFor(x => x.Id).Cascade(CascadeMode.StopOnFirstFailure)
                .Must(IsValidGuid).WithMessage($"{App.ResMan.GetString("ValidationBadId")}")
                .Must(IsNotDuplicateGuid).WithMessage($"{App.ResMan.GetString("ValidationIdAlreadyExists")}");

            RuleFor(x => x.GameId).Cascade(CascadeMode.StopOnFirstFailure)
                .Must(ValidateInteger).WithMessage($"{App.ResMan.GetString("ValidationBadGameID")}");

            RuleFor(x => x.SourceType).Must((x, y) => IsDefinedInEnum(x.SourceType, y.GetType()))
                .WithMessage($"{App.ResMan.GetString("ValidationBadSourceType")}");

            RuleFor(x => x.ExeType).Must((x, y) => IsDefinedInEnum(x.ExeType, y.GetType()))
                .WithMessage($"{App.ResMan.GetString("ValidationBadExeType")}");

            RuleFor(x => x.LastPlayed).Must(ValidateDateTime).WithMessage($"{App.ResMan.GetString("ValidationBadLastPlayed")}");

            RuleFor(x => x.PlayTime).Must(ValidateTimeSpan).WithMessage($"{App.ResMan.GetString("ValidationBadPlayTime")}");


            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure).ExeValidation();

            RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure).IcoValidation().Unless(x =>
                string.IsNullOrEmpty(x.IconPath) || string.IsNullOrWhiteSpace(x.IconPath));

            RuleFor(x => x.Arguments).Cascade(CascadeMode.StopOnFirstFailure).ArgsValidation().Unless(x =>
                string.IsNullOrEmpty(x.Arguments) || string.IsNullOrWhiteSpace(x.Arguments));

            RuleForEach(x => x.Categories).Must(ValidationHelpers.ContainsIllegalCharacters)
                .WithMessage(App.ResMan.GetString("ValidationArgumentsIllegalChars"));

        }

        public bool IsValidGuid(Guid guid)
        {
            bool isValid = Guid.TryParse(guid.ToString(), out _);
            if (isValid && guid.Equals(Guid.Empty))
            {
                isValid = false;
            }
            return isValid;
        }

        private bool IsNotDuplicateGuid(Guid id)
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return false;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query().Select(x => x.Id).ToList();
                return !dbUserData.Contains(id);
            }
        }

        private bool ValidateInteger(Stringable<int> id)
        {
            var result = int.TryParse(id.StringValue, out _);
            return result;
        }


        private  bool IsDefinedInEnum(Enum value, Type enumType)
        {
            if (value.GetType() != enumType)
                return false;

            return Enum.IsDefined(enumType, value);
        }

        private bool ValidateDateTime(DateTime dateTime)
        {
            var result = DateTime.TryParse(dateTime.ToString(CultureInfo.InvariantCulture), out _);
            return result;
        }

        private bool ValidateTimeSpan(TimeSpan ts)
        {
            var result = TimeSpan.TryParse(ts.ToString(), out _);
            return result;
        }
    }


}
