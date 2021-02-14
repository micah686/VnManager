// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using Sentry;
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
        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;

        public ModifyGamePathViewModel(IWindowManager windowManager, IDialogService dialogService, IModelValidator<ModifyGamePathViewModel> validator): base(validator)
        {
            DisplayName = App.ResMan.GetString("UpdatePaths");
            _windowManager = windowManager;
            _dialogService = dialogService;
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
            ExePath = FileDialogHelper.BrowseExe(_dialogService, this);
        }

        public void BrowseIcon()
        {
            IconPath = FileDialogHelper.BrowseIcon(_dialogService, this);
        }

        public void BrowseCover()
        {
            CoverPath = FileDialogHelper.BrowseCover(_dialogService, _windowManager, this);
        }

        
        /// <summary>
        /// Update the data in the Database
        /// <see cref="UpdateAsync"/>
        /// </summary>
        /// <returns></returns>
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

                UpdateCover(CoverPath);

                _windowManager.ShowMessageBox(App.ResMan.GetString("GameUpdatedMsg"), App.ResMan.GetString("GameUpdated"));
            }
        }

        /// <summary>
        /// Update the cover image
        /// </summary>
        /// <param name="cover"></param>
        private void UpdateCover(string cover)
        {
            try
            {
                if (cover == string.Empty || !File.Exists(cover))
                {
                    return;
                }
                const int maxFileSize = 5242880;//5MB
                var length = new FileInfo(cover).Length;
                if(length <= maxFileSize)
                {
                    return;
                }
                var png = Image.FromFile(cover);
                png.Save($"{Path.Combine(App.AssetDirPath, @"sources\noSource\images\cover\")}{SelectedGame.Id}.png");
                png.Dispose();
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to update cover image");
                SentrySdk.CaptureException(e);
            }
        }
        
        /// <summary>
        /// Recheck the validation
        /// <see cref="RecheckValidationAsync"/>
        /// </summary>
        /// <returns></returns>
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
