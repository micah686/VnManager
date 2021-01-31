// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGamePathViewModel: Screen
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string Arguments { get; set; }
        public string Title { get; set; }
        public string CoverPath { get; set; }
        public bool EnableArgs { get; set; }

        public Visibility NoSourceVisibility { get; set; } = Visibility.Collapsed;

        internal UserDataGames SelectedGame;
        private readonly OpenFileDialogSettings _defaultOpenFileDialogSettings;
        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;

        public ModifyGamePathViewModel(IWindowManager windowManager, IDialogService dialogService, IModelValidator<ModifyGamePathViewModel> validator): base(validator)
        {
            DisplayName = App.ResMan.GetString("UpdatePaths");
            _windowManager = windowManager;
            _dialogService = dialogService;
            _defaultOpenFileDialogSettings = new OpenFileDialogSettings
            {
                FileName = "",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
        }

        protected override void OnViewLoaded()
        {
            SetOriginalValues();
        }

        private void SetOriginalValues()
        {
            ExePath = SelectedGame.ExePath;
            IconPath = SelectedGame.IconPath;
            Arguments = SelectedGame.Arguments;
            EnableArgs = !string.IsNullOrEmpty(Arguments);
            if (SelectedGame.SourceType == AddGameSourceType.NoSource)
            {
                NoSourceVisibility = Visibility.Visible;
                Title = SelectedGame.Title;
                CoverPath = SelectedGame.CoverPath;
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

        public void BrowseCover()
        {
            var settings = _defaultOpenFileDialogSettings;
            settings.Title = App.ResMan.GetString("BrowseForCover") ?? string.Empty;
            settings.DefaultExt = ".jpg";
            settings.Filter = "Images (*.jpg,*.png)|*.jpg;*.png";

            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                if (ImageHelper.IsValidImage(settings.FileName))
                {
                    CoverPath = settings.FileName;
                }
                else
                {
                    _windowManager.ShowMessageBox(App.ResMan.GetString("ValidationNotValidImage"));
                }
                
            }
        }

        public async Task UpdateAsync()
        {
            bool result = await ValidateAsync();
            if (result)
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }

                if (EnableArgs==false)
                {
                    Arguments = string.Empty;
                }
                
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                var entry = dbUserData.Query().Where(x => x.Id == SelectedGame.Id)
                    .FirstOrDefault();
                if (entry != null)
                {
                    entry.ExePath = ExePath ?? string.Empty;
                    entry.IconPath = IconPath ?? string.Empty;
                    entry.Arguments = Arguments ?? string.Empty;
                    entry.Title = Title ?? string.Empty;
                    entry.CoverPath = CoverPath ?? string.Empty;
                    dbUserData.Update(entry);

                }
                _windowManager.ShowMessageBox(App.ResMan.GetString("GameUpdatedMsg"), App.ResMan.GetString("GameUpdated"));
            }
        }

        public async Task RecheckValidationAsync()
        {
            await ValidateAsync();
        }
        
    }

    public class ModifyGamePathViewModelValidator : AbstractValidator<ModifyGamePathViewModel>
    {
        public ModifyGamePathViewModelValidator()
        {
            RuleFor(x => x.ExePath).Cascade(CascadeMode.Stop).ExeValidation();
            When(x => !string.IsNullOrEmpty(x.IconPath)|| !string.IsNullOrWhiteSpace(x.IconPath), () =>
            {
                RuleFor(x => x.IconPath).Cascade(CascadeMode.Stop).IcoValidation();
            });

            When(x => x.EnableArgs, () =>
            {
                RuleFor(x => x.Arguments).Cascade(CascadeMode.Stop).ArgsValidation();
            });

            

            When(x => x.NoSourceVisibility == Visibility.Visible, () =>
            {
                RuleFor(x => x.Title).Cascade(CascadeMode.Stop)
                    .Must(ValidationHelpers.ContainsIllegalCharacters).WithMessage(App.ResMan.GetString("ValidationArgumentsIllegalChars"));

                RuleFor(x => x.CoverPath).Cascade(CascadeMode.Stop)
                    .Must(ValidateFiles.EndsWithJpgOrPng).WithMessage(App.ResMan.GetString("ValidationImagePathNotValid"))
                    .Must(ImageHelper.IsValidImage).WithMessage(App.ResMan.GetString("ValidationNotValidImage"));
            });

        }
    }
}
