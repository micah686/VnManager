using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using VndbSharp.Models;
using VnManager.Helpers;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels.Dialogs
{
    public class ImportViewModel: Screen
    {
        public BindableCollection<UserDataGames> UserDataGamesCollection { get; set; } = new BindableCollection<UserDataGames>();
        public bool IsDataGridEnabled { get; set; } = false;

        private bool _didCancelImport = false;
        public bool IsImportingVisible { get; set; } = false;

        public string ImportMessage { get; set; }
        public string CurrentProgressMsg { get; set; }
        public string TotalProgressMsg { get; set; }
        public double TotalProgress { get; set; }
        public bool IsImportProcessing { get; set; }
        public bool BlockClosing { get; set; } = false;


        private readonly List<int> _vndbGameIds= new List<int>();

        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;
        private readonly ImportUserDataValidator _validator = new ImportUserDataValidator();

        public ImportViewModel(IWindowManager windowManager, IDialogService dialogService)
        {
            _windowManager = windowManager;
            _dialogService = dialogService;
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
                App.Logger.Warning(e, "Failed to remove row from import tool");
                //throw;
            }


        }

        public async Task ValidateDataAsync()
        {
            int errorCount = 0;
            
            
            foreach (var item in UserDataGamesCollection)
            {
                item.ClearAllErrors();
                var result = await _validator.ValidateAsync(item);
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
                await ImportDataAsync();

            }

        }



        private async Task ImportDataAsync()
        {
            try
            {
                var entryCount = UserDataGamesCollection.Count;

                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return;

                BlockClosing = true;
                IsDataGridEnabled = true;
                IsImportProcessing = true;
                for (int i = 0; i < entryCount; i++)
                {
                    if (_didCancelImport == true)
                    {
                        BlockClosing = false;
                        IsDataGridEnabled = true;
                        return;
                    }
                    
                    int errorCount = 0;

                    int maxId;

                    using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                    {
                        maxId = db.GetCollection<UserDataGames>("UserData_Games").Query().OrderByDescending(x => x.Index).Select(x => x.Index)
                            .FirstOrDefault();
                    }

                    var entry = UserDataGamesCollection.First();
                    var result = await _validator.ValidateAsync(entry);
                    foreach (var error in result.Errors)
                    {
                        errorCount += 1;
                        entry.SetError(error.PropertyName, error.ErrorMessage);
                    }

                    if (errorCount > 0)
                    {
                        
                        _windowManager.ShowMessageBox($"{App.ResMan.GetString("ValidationFailedRecheck")}");
                        return;
                    }

                    maxId += 1;
                    entry.Index = maxId;

                    await Task.Run(() =>
                    {
                        using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                        var dbUserData = db.GetCollection<UserDataGames>("UserData_Games");
                        dbUserData.Insert(entry);
                    });

                    if (entry.SourceType == AddGameSourceType.Vndb)
                    {
                        _vndbGameIds.Add(entry.GameId);
                    }

                    UserDataGamesCollection.RemoveAt(0);
                    UserDataGamesCollection.Refresh();

                    BlockClosing = false;
                }

                BlockClosing = false;
                IsDataGridEnabled = true;
                IsImportProcessing = false;
                await UpdateVndbDataAsync(_vndbGameIds);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                
                throw;
            }
            


        }


        private async Task UpdateVndbDataAsync(List<int> gameIds)
        {
            RequestOptions ro = new RequestOptions() { Count = 25 };
            var getData = new MetadataProviders.Vndb.GetVndbData();
            var saveData = new MetadataProviders.Vndb.SaveVnDataToDb();
            using (var client = new VndbSharp.Vndb(true))
            {
                IsImportingVisible = true;
                IsImportProcessing = true;
                ImportMessage = $"{App.ResMan.GetString("ImportDbStarting")}";
                TotalProgressMsg = $"{App.ResMan.GetString("TotalProgressColon")} 0/{gameIds.Count}";
                var totalIncrement = 100 / gameIds.Count;
                var count = 0;


                foreach (var strId in gameIds)
                {
                    if (_didCancelImport) break;
                    BlockClosing = true;
                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg1")}";
                    var id = (uint)strId;
                    var visualNovel = await getData.GetVisualNovelAsync(client, id);
                    var releases = await getData.GetReleasesAsync(client, id, ro);
                    uint[] producerIds = releases.SelectMany(x => x.Producers.Select(y => y.Id)).Distinct().ToArray();
                    var producers = await getData.GetProducersAsync(client, producerIds, ro);
                    var characters = await getData.GetCharactersAsync(client, id, ro);
                    uint[] staffIds = visualNovel.Staff.Select(x => x.StaffId).Distinct().ToArray();
                    var staff = await getData.GetStaffAsync(client, staffIds, ro);

                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg2")}";
                    saveData.SaveVnInfo(visualNovel);
                    saveData.SaveVnCharacters(characters, id);
                    saveData.SaveVnReleases(releases);
                    saveData.SaveProducers(producers);
                    saveData.SaveStaff(staff, (int)id);

                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg3")}";
                    await DownloadVndbContent.DownloadCoverImageAsync(id);
                    await DownloadVndbContent.DownloadCharacterImagesAsync(id);
                    CurrentProgressMsg = $"{App.ResMan.GetString("ImportDbProg4")}";
                    await DownloadVndbContent.DownloadScreenshotsAsync(id);


                    TotalProgress += totalIncrement;
                    count = count + 1;
                    TotalProgressMsg = $"{App.ResMan.GetString("TotalProgressColon")} {count}/ {gameIds.Count}";
                    BlockClosing = false;
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
            IsDataGridEnabled = true;
            BlockClosing = false;
            IsImportProcessing = false;
            ImportMessage = $"{App.ResMan.GetString("ImportDbCancelWorking")}";
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

        #region Helpers
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


        private bool IsDefinedInEnum(Enum value, Type enumType)
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
        #endregion
    }


}
