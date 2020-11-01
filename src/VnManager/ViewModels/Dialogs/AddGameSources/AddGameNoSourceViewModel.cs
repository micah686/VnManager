using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.MetadataProviders;
using VnManager.Models;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameNoSourceViewModel: Screen
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
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

        public AddGameNoSourceViewModel(IDialogService dialogService, IModelValidator<AddGameNoSourceViewModel> validator): base(validator)
        {
            _dialogService = dialogService;
        }


        public void BrowseExe()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "Browse for Game",
                DefaultExt = ".exe",
                Filter = "Applications (*.exe)|*.exe",
                FileName = "",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                ExePath = settings.FileName;
            }
        }

        public void BrowseIcon()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "Browse for Game Icon",
                DefaultExt = ".ico",
                Filter = "Icons (*.ico,*.exe)|*.ico;*.exe",
                FileName = "",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                IconPath = settings.FileName;
            }
        }

        public async Task SubmitAsync()
        {
            bool result = await ValidateAsync();
            if (result == true)
            {
                SetGameDataEntry();
                var parent = (AddGameMainViewModel)Parent;
                parent.RequestClose(true);
            }
        }

        private void SetGameDataEntry()
        {
            var gameEntry = new AddItemDbModel
            {
                SourceType = AddGameSourceType.NoSource,
                ExeType = ExeTypeEnum.Normal,
                IsCollectionEnabled = false,
                ExeCollection = null,
                GameId = 0,
                ExePath = ExePath,
                IsIconEnabled = IsIconChecked,
                IconPath = IconPath,
                IsArgumentsEnabled = IsArgsChecked,
                ExeArguments = ExeArguments
            };
            MetadataCommon.SetGameEntryData(gameEntry);
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
            if (cred == null || cred.UserName.Length < 1) return false;
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
