using FluentValidation;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using System;
using System.Collections.Generic;
using System.Text;
using VnManager.Helpers;

namespace VnManager.ViewModels.Dialogs
{
    public class AddGameMultiViewModel: Screen
    {
        public BindableCollection<MultiExeGamePaths> GameCollection { get; set; } = new BindableCollection<MultiExeGamePaths>();
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        private bool _isIconChecked;
        public bool IsIconChecked
        {
            get => _isIconChecked;
            set
            {
                if(value== false)
                {
                    IconPath = string.Empty;
                }
                SetAndNotify(ref _isIconChecked, value);
            }
        }

        private bool _isArgsChecked;
        public bool IsArgsChecked
        {
            get => _isArgsChecked;
            set
            {
                if(value == false)
                {
                    ExeArguments = string.Empty;
                }
                SetAndNotify(ref _isArgsChecked, value);
            }
        }


        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        public AddGameMultiViewModel(IWindowManager windowManager, IModelValidator<AddGameMultiViewModel> validator, IDialogService dialogService) : base(validator)
        {
            _windowManager = windowManager;
            _dialogService = dialogService;
        }

        public void Add()
        { 
            var validator = new AddGameMultiViewModelValidator();
            this.Validate();
            bool result = validator.Validate(this).IsValid;
            if (result == true)
            {
                var exe = ExePath;
                var icon = IconPath;
                var args = ExeArguments;

                GameCollection.Add(new MultiExeGamePaths { ExePath = exe, IconPath = icon, ArgumentsString = args });
            }
        }

        public void Remove()
        {
            if(GameCollection.Count > 0)
            {
                GameCollection.RemoveAt(GameCollection.Count -1);
            }
        }
        
        public void BrowseExePath()
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
            if(result == true)
            {
                ExePath = settings.FileName;
            }
        }

        public void BrowseIconPath()
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

        public void Submit()
        {
            
            this.RequestClose(true);
        }

        public void Cancel()
        {
            this.RequestClose(false);
        }

        
    }

    public class AddGameMultiViewModelValidator : AbstractValidator<AddGameMultiViewModel>
    {
        public AddGameMultiViewModelValidator()
        {
            RuleFor(x => x.ExePath).NotEmpty().WithMessage("Exe Path cannot be empty");            

            RuleFor(x => x.ExePath).Must(ValidateFiles.EndsWithExe).When(x => !string.IsNullOrWhiteSpace(x.ExePath) || !string.IsNullOrEmpty(x.ExePath)).WithMessage("Not a valid path to exe");
            RuleFor(x => x.ExePath).Must(ValidateFiles.ValidateExe).When(x => !string.IsNullOrWhiteSpace(x.ExePath) || !string.IsNullOrEmpty(x.ExePath)).WithMessage("Not a valid Executable");

            RuleFor(x => x.IconPath).Must(ValidateFiles.EndsWithIcoOrExe).When(x => !string.IsNullOrWhiteSpace(x.IconPath) || !string.IsNullOrEmpty(x.IconPath)).WithMessage("Not a valid path to icon");

            When(x => x.IsArgsChecked == true, () =>
              {
                  RuleFor(x => x.ExeArguments).NotEmpty().WithMessage("Arguments cannot be empty");
              });

            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).NotEmpty().WithMessage("Icon Path cannot be empty");
            });

        }
    }


    public class MultiExeGamePaths
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ArgumentsString { get; set; }
    }
}
