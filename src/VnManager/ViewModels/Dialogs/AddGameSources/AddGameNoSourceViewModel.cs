using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
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




        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;

        public AddGameNoSourceViewModel(IContainer container, IWindowManager windowManager,
            IDialogService dialogService, IModelValidator<AddGameNoSourceViewModel> validator): base(validator)
        {
            _container = container;
            _windowManager = windowManager;
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

        public async Task Submit()
        {
            bool result = await ValidateAsync();
            if (result == true)
            {
                var parent = (AddGameMainViewModel)Parent;
                parent.RequestClose(true);
            }
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
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());

            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage(rm.GetString("ValidationExePathEmpty"))
                .Must(ValidateFiles.EndsWithExe).WithMessage(rm.GetString("ValidationExePathNotValid"))
                .Must(ValidateFiles.ValidateExe).WithMessage(rm.GetString("ValidationExeNotValid"))
                .Must(IsNotDuplicateExe).WithMessage(rm.GetString("ExeAlreadyExistsInDb"));


            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty().WithMessage(rm.GetString("ValidationIconPathEmpty"))
                    .Must(ValidateFiles.EndsWithIcoOrExe).WithMessage(rm.GetString("ValidationIconPathNotValid"));
            });

            When(x => x.IsArgsChecked == true, () =>
            {
                RuleFor(x => x.ExeArguments).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty().WithMessage(rm.GetString("ValidationArgumentsEmpty"))
                    .Must(AddGameMultiViewModelValidator.ContainsIllegalCharacters).Unless(x => x.ExeArguments == "")
                        .WithMessage(rm.GetString("ValidationArgumentsIllegalChars"));
            });

        }


        private bool IsNotDuplicateExe(string exePath)
        {
            using (var db = new LiteDatabase(App.GetDatabaseString()))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query()
                    .Where(x => x.SourceType == AddGameMainViewModel.AddGameSourceType.NoSource).ToEnumerable();
                var count = dbUserData.Count(x => x.ExePath == exePath);
                return count <= 0;
            }
        }
    }
}
