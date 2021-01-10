using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
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

        public Visibility NoSourceVisibility { get; set; } = Visibility.Collapsed;

        private readonly OpenFileDialogSettings _defaultOpenFileDialogSettings;
        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;

        public ModifyGamePathViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService, IModelValidator<ModifyGamePathViewModel> validator): base(validator)
        {
            DisplayName = App.ResMan.GetString("UpdatePaths");
            _container = container;
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
            ExePath = ModifyGameHostViewModel.SelectedGame.ExePath;
            IconPath = ModifyGameHostViewModel.SelectedGame.IconPath;
            Arguments = ModifyGameHostViewModel.SelectedGame.Arguments;

            if (ModifyGameHostViewModel.SelectedGame.SourceType == AddGameSourceType.NoSource)
            {
                NoSourceVisibility = Visibility.Visible;
                Title = ModifyGameHostViewModel.SelectedGame.Title;
                CoverPath = ModifyGameHostViewModel.SelectedGame.CoverPath;
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

        public async Task Update()
        {
            bool result = await ValidateAsync();
            if (result)
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return;
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                var entry = dbUserData.Query().Where(x => x.Id == ModifyGameHostViewModel.SelectedGame.Id)
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
            
            RuleFor(x => x.Arguments).Cascade(CascadeMode.Stop).ArgsValidation();

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
