using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Windows
{
    public class ImportExportDataViewModel: Screen
    {
        public BindableCollection<UserDataGames> UserDataGamesCollection { get; set; }
        public bool IsDataGridEnabled { get; set; } = false;
        
        
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public ImportExportDataViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService, IModelValidator<ImportExportDataViewModel> validator):base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;
            UserDataGamesCollection = new BindableCollection<UserDataGames>();
        }

        public void ExportData()
        {

            string savePath = string.Empty;
            var settings = new FolderBrowserDialogSettings()
            {
                Description = "Choose a location to export the user data"
            };
            bool? result = _dialogService.ShowFolderBrowserDialog(this, settings);
            if (result == true)
            {
                savePath = settings.SelectedPath;
            }

            var fileName = $@"{savePath}\VnManager_Export_{DateTime.UtcNow:yyyy-MMMM-dd}.db";
            var cred = CredentialManager.GetCredentials("VnManager.DbEnc");
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
                    _windowManager.ShowMessageBox($"User Data exported to: \n{fileName}", "User Data exported");

                }


            }
        }


        public void FillDatabaseWithDupe()
        {
            string savePath = string.Empty;
            var settings = new FolderBrowserDialogSettings()
            {
                Description = "Choose a location to export the user data"
            };
            bool? result = _dialogService.ShowFolderBrowserDialog(this, settings);
            if (result == true)
            {
                savePath = settings.SelectedPath;
            }

            var fileName = $@"{savePath}\VnManager_Export_{DateTime.UtcNow:yyyy-MMMM-dd}.db";
            using (var exportDatabase = new LiteDatabase(fileName))
            {
                var exportUserData = exportDatabase.GetCollection<UserDataGames>("UserData_Games");
                for (int i = 0; i < 7; i++)
                {
                    var userdata = new UserDataGames()
                    {
                        Id = Guid.NewGuid(),
                        GameId = i,
                        GameName = $"Game{i}",
                        SourceType = AddGameMainViewModel.AddGameSourceType.NotSet,
                        LastPlayed = DateTime.MinValue,
                        PlayTime = TimeSpan.MinValue,
                        Categories = null,
                        ExePath = @"C:\Users\Micah\Downloads\vs_Community.exe",
                        IconPath = String.Empty,
                        Arguments = "-quiet"
                    };
                    exportUserData.Insert(userdata);
                }
                _windowManager.ShowMessageBox($"User Data exported to: \n{fileName}", "User Data exported");

            }
        }

        public void BrowseImportDump()
        {
            string filename = "VnManager_Export_YYYY-Month-DD.db";
            var settings = new OpenFileDialogSettings
            {
                Title = "Browse for Database Dump",
                DefaultExt = ".db",
                Filter = "Database Dump (*.db)|*.db",
                FileName = filename,
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                //IconPath = settings.FileName;
                var filepath = settings.FileName;
                //using (var db = new LiteDatabase($"{filepath}"))
                //{
                //    IEnumerable<UserDataGames> dbUserData = db.GetCollection<UserDataGames>("UserData_Games").FindAll();
                //    UserDataGamesCollection.AddRange(dbUserData);
                //    IsDataGridEnabled = true;
                //}
                LoadDatabase(filepath);

            }
        }

        private void LoadDatabase(string filePath)
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

        public void BrowseExe()
        {

        }

        public void BrowseIcon()
        {

        }

        public void ImportData()
        {
            //var validator = new ImportExportDataViewModelValidator();
            //var result = validator.Validate(this);
            //Validate();

            //foreach (var model in UserDataGamesCollection)
            //{
            //    var validator = new ImportExportDataViewModelValidator();
            //    var result = validator.Validate(model);
            //}

            var validator = new ImportUserDataValidator();
            foreach (var item in UserDataGamesCollection)
            {
                var result = validator.Validate(item);
                foreach (var error in result.Errors)
                {
                    item.SetError(error.PropertyName, error.ErrorMessage);
                }
                
            }
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
            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure).ExeValidation();

            RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure).IcoValidation();

            RuleFor(x => x.Arguments).Cascade(CascadeMode.StopOnFirstFailure).ArgsValidation();


        }
    }
}
