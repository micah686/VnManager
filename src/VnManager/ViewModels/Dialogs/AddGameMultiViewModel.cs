using FluentValidation;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
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
                SetAndNotify(ref _isIconChecked, value);
                if (_isIconChecked == false)
                {
                    IconPath = string.Empty;
                    HideIconError = true;
                    Validate();
                    HideIconError = false;
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
                    HideArgumentsError = true;
                    Validate();
                    HideArgumentsError = false;
                }
            }
        }

        public bool ShowValidationErrors { get; private set; } = true;
        public bool HideArgumentsError { get; private set; } = false;
        public bool HideIconError { get; private set; } = false;

        private readonly IDialogService _dialogService;
        public AddGameMultiViewModel(IWindowManager windowManager, IModelValidator<AddGameMultiViewModel> validator, IDialogService dialogService) : base(validator)
        {
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

                ShowValidationErrors = false;//prevent validation errors from showing up after a sucessful Add
                ExePath = string.Empty;
                IconPath = string.Empty;
                ExeArguments = string.Empty;
                Validate();
                ShowValidationErrors = true;
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
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure)
               .NotEmpty().Unless(x => x.ShowValidationErrors == false).WithMessage(rm.GetString("ValidationExePathEmpty"))
               .Must(ValidateFiles.EndsWithExe).Unless(x => x.ShowValidationErrors == false).WithMessage(rm.GetString("ValidationExePathNotValid"))
               .Must(ValidateFiles.ValidateExe).Unless(x => x.ShowValidationErrors == false).WithMessage(rm.GetString("ValidationExeNotValid"));
                        
            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty().Unless(x => x.ShowValidationErrors == false && x.HideIconError == true).WithMessage(rm.GetString("ValidationIconPathEmpty"))
                    .Must(ValidateFiles.EndsWithIcoOrExe).Unless(x => x.ShowValidationErrors == false).WithMessage(rm.GetString("ValidationIconPathNotValid"));

            });

            When(x => x.IsArgsChecked == true, () =>
              {                  
                  RuleFor(x => x.ExeArguments).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty().Unless(x => x.ShowValidationErrors == false && x.HideArgumentsError == true).WithMessage(rm.GetString("ValidationArgumentsEmpty"))
                    .Must(ContainsIllegalCharacters).Unless(x => x.ShowValidationErrors == false).WithMessage(rm.GetString("ValidationArgumentsIllegalChars"));
              });
                        
        }

        public static bool ContainsIllegalCharacters(string format)
        {
            if (format == null) return false;
            string allowableLetters = $@"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890/\-_ !?;:'+={'"'}";

            foreach (char c in format)
            {
                if (!allowableLetters.Contains(c))
                    return false;
            }

            return true;
        }
    }


    public class MultiExeGamePaths
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ArgumentsString { get; set; }
    }
}
