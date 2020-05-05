using System;
using System.Globalization;
using System.Windows.Data;
using Stylet;
using MvvmDialogs;
using FluentValidation;
using StyletIoC;
using VnManager.ViewModels.Dialogs;
using System.Collections.Generic;
using System.Linq;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using VnManager.Helpers;

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        private List<MultiExeGamePaths> _exeCollection = new List<MultiExeGamePaths>();

        #region Properties
        public string SourceSite { get; set; }
        public int? VnId { get; set; }        
        public string VnName { get; set; }
        public bool IsNameChecked { get; set; }        
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        public bool CanChangeVnName { get; set; }
        public bool HideArgumentsError { get; private set; } = false;
        public bool HideIconError { get; private set; } = false;


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

        private ExeTypesEnum _exeTypes;
        public ExeTypesEnum ExeTypes
        {
            get { return _exeTypes; }
            set
            {
                SetAndNotify(ref _exeTypes, value);
                if(_exeTypes != ExeTypesEnum.Collection)
                {
                    IsNotExeCollection = true;
                }
                else
                {
                    IsNotExeCollection = false;
                }
            }
        }



        private bool _isNotExeCollection;
        public bool IsNotExeCollection
        {
            get { return _isNotExeCollection; }
            set
            {
                ExeArguments = string.Empty;
                IconPath = string.Empty;
                IsArgsChecked = false;
                IsIconChecked = false;
                SetAndNotify(ref _isNotExeCollection, value);
            }
        }

        #endregion



        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;
        public AddGameViewModel(IContainer container, IWindowManager windowManager, IModelValidator<AddGameViewModel> validator, IDialogService dialogService) : base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;
            IsNotExeCollection = true;
            CanChangeVnName = true;
        }

        public void Search()
        {
            CanChangeVnName = false;
            var validator = new AddGameViewModelValidator();
            this.Validate();
            bool result = validator.Validate(this).IsValid;
        }

        public void ResetName()
        {
            CanChangeVnName = true;
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

        public void ManageExes()
        {
            var multivm = _container.Get<AddGameMultiViewModel>();
            var result = _windowManager.ShowDialog(multivm).Value;
            if(result == true)
            {
                if(multivm.GameCollection != null)
                {
                    _exeCollection.Clear();
                    _exeCollection.AddRange(from item in multivm.GameCollection
                         select new MultiExeGamePaths { ExePath = item.ExePath, IconPath = item.IconPath, ArgumentsString = item.ArgumentsString });
                }

            }
            multivm.Remove();
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

        public void Submit()
        {
            var validator = new AddGameViewModelValidator();
            this.Validate();
            bool result = validator.Validate(this).IsValid;
            if(result == true)
            {
                this.RequestClose();
            }
        }

        public void Cancel()
        {
            RequestClose();
        }


    }

    public enum ExeTypesEnum { Normal, Launcher, Collection}

    public class AddGameViewModelValidator : AbstractValidator<AddGameViewModel>
    {
        public AddGameViewModelValidator()
        {
            When(x => x.IsNameChecked == false, () =>
            {
                RuleFor(x => x.VnId).NotEmpty().WithMessage("VnId Cannot be Emtpy");
                RuleFor(x => x.VnId).GreaterThan(0).WithMessage("VnId must be greater than 0");
            });

            When(x => x.IsNameChecked == true, () =>
            {
                RuleFor(x => x.CanChangeVnName).NotEqual(true).WithMessage("A selection from the list of VN names is required");
                RuleFor(x => x.VnName).NotEmpty().WithMessage("Vn Name cannot be empty");
            });


            RuleFor(x => x.ExePath).NotEmpty().WithMessage("Exe Path cannot be empty");
            RuleFor(x => x.ExePath).Must(ValidateFiles.EndsWithExe).When(x => !string.IsNullOrWhiteSpace(x.ExePath) || !string.IsNullOrEmpty(x.ExePath)).WithMessage("Not a valid path to exe");
            RuleFor(x => x.ExePath).Must(ValidateFiles.ValidateExe).When(x => !string.IsNullOrWhiteSpace(x.ExePath) || !string.IsNullOrEmpty(x.ExePath)).WithMessage("Not a valid Executable");

            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).NotEmpty().Unless(x => x.HideIconError == true).WithMessage("Icon Path cannot be empty");
                RuleFor(x => x.IconPath).Must(ValidateFiles.EndsWithIcoOrExe).Unless(x => x.HideIconError == true)
                    .When(x => !string.IsNullOrWhiteSpace(x.IconPath) || !string.IsNullOrEmpty(x.IconPath)).WithMessage("Not a valid path to icon");
            });

            When(x => x.IsArgsChecked == true && x.ExeArguments == "", () =>
            {
                RuleFor(x => x.ExeArguments).NotEmpty().Unless(x => x.HideArgumentsError == true).WithMessage("Arguments cannot be empty");
                RuleFor(x => x.ExeArguments).Must(AddGameMultiViewModelValidator.ContainsIllegalCharacters).Unless(x => x.HideArgumentsError == true)
                    .When(x => !string.IsNullOrWhiteSpace(x.ExeArguments) || !string.IsNullOrEmpty(x.ExeArguments)).WithMessage("Illegal characters detected");
            });




        }
    }

    public class VnIdNameBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
                return "Vn Name:";
            else
                return "Vn ID:";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.OfType<IConvertible>().All(System.Convert.ToBoolean);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
