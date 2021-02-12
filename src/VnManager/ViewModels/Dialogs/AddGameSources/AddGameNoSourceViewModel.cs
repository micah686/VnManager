// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameNoSourceViewModel: Screen
    {
        private readonly OpenFileDialogSettings _defaultOpenFileDialogSettings;
        public string Title { get; set; }
        public string CoverPath { get; private set; }
        public bool IsLockDown { get; set; }
        public string ExePath { get; private set; }
        public string IconPath { get; private set; }
        public string ExeArguments { get; set; }

        private bool _isIconChecked;
        public bool IsIconChecked
        {
            get => _isIconChecked;
            set
            {
                SetAndNotify(ref _isIconChecked, value);
                if (_isIconChecked == false)
                {
                    IconPath = string.Empty;
                }
            }
        }

        private bool _isArgsChecked;
        public bool IsArgsChecked
        {
            get => _isArgsChecked;
            set
            {
                SetAndNotify(ref _isArgsChecked, value);
                if (_isArgsChecked == false)
                {
                    ExeArguments = string.Empty;
                }
            }
        }


        private readonly IDialogService _dialogService;
        private readonly INavigationController _navigationController;
        public AddGameNoSourceViewModel(IDialogService dialogService, INavigationController navigationController, IModelValidator<AddGameNoSourceViewModel> validator): base(validator)
        {
            _dialogService = dialogService;
            _navigationController = navigationController;

            _defaultOpenFileDialogSettings = new OpenFileDialogSettings
            {
                FileName = "",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
        }

        public void BrowseCover()
        {
            var settings = _defaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseForCover") ?? string.Empty;
            settings.DefaultExt = ".jpg";
            settings.Filter = "Images (*.jpg,*.png)|*.jpg;*.png";

            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                CoverPath = settings.FileName;
            }
        }

        public void BrowseExe()
        {
            var settings = _defaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseExe") ?? string.Empty;
            settings.DefaultExt = ".exe";
            settings.Filter = "Applications (*.exe)|*.exe";

            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                ExePath = settings.FileName;
            }
        }

        public void BrowseIcon()
        {
            var settings = _defaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseForIcon") ?? string.Empty;
            settings.DefaultExt = ".ico";
            settings.Filter = "Icons (*.ico,*.exe)|*.ico;*.exe";

            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                IconPath = settings.FileName;
            }

        }

        public async Task SubmitAsync()
        {
            IsLockDown = true;
            var parent = (AddGameMainViewModel)Parent;
            parent.CanChangeSource = false;
            bool result = await ValidateAsync();
            if (result == true)
            {
                SetGameDataEntryAsync();
                parent.RequestClose(true);
                _navigationController.NavigateToMainGrid();
            }
            parent.CanChangeSource = true;
            IsLockDown = false;
        }

        private void SetGameDataEntryAsync()
        {
            const int maxFileSize = 5242880;//5MB
            var entry = AddGameMainViewModel.GetDefaultUserDataEntry;
            entry.SourceType = AddGameSourceType.NoSource;
            entry.ExePath = ExePath;
            entry.Arguments = ExeArguments;
            entry.IconPath = IconPath;
            entry.CoverPath = CoverPath;
            entry.Title = Title;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                dbUserData.Insert(entry);
            }

            var length = new FileInfo(CoverPath).Length;
            if (length <= maxFileSize)
            {
                return;
            }
            var png = Image.FromFile(CoverPath);
            png.Save($"{Path.Combine(App.AssetDirPath, @"sources\noSource\images\cover\")}{entry.Id}.png");
            png.Dispose();



        }

        public void Cancel()
        {
            var parent = (AddGameMainViewModel)Parent;
            parent.RequestClose();
        }

    }


    public class AddGameNoSourceViewModelValidator : AbstractValidator<AddGameNoSourceViewModel>
    {

        public AddGameNoSourceViewModelValidator()
        {
            RuleFor(x => x.Title).Cascade(CascadeMode.Stop)
                .Must(ValidationHelpers.ContainsIllegalCharacters).WithMessage(App.ResMan.GetString("ValidationArgumentsIllegalChars"));

            RuleFor(x => x.CoverPath).Cascade(CascadeMode.Stop)
                .Must(ValidateFiles.EndsWithJpgOrPng).WithMessage(App.ResMan.GetString("ValidationImagePathNotValid"))
                .Must(ImageHelper.IsValidImage).WithMessage(App.ResMan.GetString("ValidationNotValidImage"));

            RuleFor(x => x.ExePath).Cascade(CascadeMode.Stop).ExeValidation()
                .Must(IsNotDuplicateExe).WithMessage(App.ResMan.GetString("ExeAlreadyExistsInDb"));

            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).Cascade(CascadeMode.Stop).IcoValidation();
            });

            When(x => x.IsArgsChecked == true, () =>
            {
                RuleFor(x => x.ExeArguments).Cascade(CascadeMode.Stop).ArgsValidation();
            });

        }


        private static bool IsNotDuplicateExe(string exePath)
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return false;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                    .Where(x => x.SourceType == AddGameSourceType.NoSource).ToEnumerable();
                var count = dbUserData.Count(x => x.ExePath == exePath);
                return count <= 0;
            }
        }
    }
}
