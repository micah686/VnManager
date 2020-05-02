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

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        private List<MultiExeGamePaths> _exeCollection = new List<MultiExeGamePaths>();
        
        public string SourceSite { get; set; }
        public int VnId { get; set; }        
        public string VnName { get; set; }
        public bool IsNameChecked { get; set; }        
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        private bool _isExeNormalChecked;
        public bool IsExeNormalChecked
        {
            get { return _isExeNormalChecked; }
            set
            {
                if(value == true)
                {
                    IsNotExeCollection = true;
                }
                
                SetAndNotify(ref _isExeNormalChecked, value);
            }
        }
        private bool _isExeLauncherChecked;
        public bool IsExeLauncherChecked
        {
            get { return _isExeLauncherChecked; }
            set
            {
                if (value == true)
                {
                    IsNotExeCollection = true;
                }
                SetAndNotify(ref _isExeLauncherChecked, value);
            }
        }
        private bool _isExeCollectionChecked;
        public bool IsExeCollectionChecked
        {
            get { return _isExeCollectionChecked; }
            set
            {
                if (value == true)
                {
                    IsNotExeCollection = false;
                }
                SetAndNotify(ref _isExeCollectionChecked, value);
            }
        }
        public bool IsCustomArgsChecked { get; set; }
        public bool IsCustomIconChecked { get; set; }
        public string ExeArguments { get; set; }

        private bool _isNotExeCollection;
        public bool IsNotExeCollection
        {
            get { return _isNotExeCollection; }
            set
            {
                ExeArguments = string.Empty;
                IconPath = string.Empty;
                IsCustomArgsChecked = false;
                IsCustomIconChecked = false;
                SetAndNotify(ref _isNotExeCollection, value);
            }
        }



        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;
        public AddGameViewModel(IContainer container, IWindowManager windowManager, IModelValidator<AddGameViewModel> validator, IDialogService dialogService) : base(validator)
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
            this.RequestClose();
        }

        public void Cancel()
        {
            RequestClose();
        }


    }

    public class AddGameViewModelValidator : AbstractValidator<AddGameViewModel>
    {
        public AddGameViewModelValidator()
        {
            RuleFor(x => x.ExePath).NotEmpty().WithMessage("Exe Path cannot be empty");
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
}
