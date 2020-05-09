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
using System.Threading;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models.VisualNovel;
using VndbSharp.Models;
using VnManager.Helpers.Vndb;
using VnManager.Utilities;
using System.Text.RegularExpressions;

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        private List<MultiExeGamePaths> _exeCollection = new List<MultiExeGamePaths>();
        public BindableCollection<string> SuggestedNamesCollection { get; private set; }

        #region Properties
        public string SourceSite { get; set; }
        public int VnId { get; set; }        
        public string VnName { get; set; }
        public bool IsNameChecked { get; set; }        
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        public bool CanChangeVnName { get; set; }
        public bool HideArgumentsError { get; private set; } = false;
        public bool HideIconError { get; private set; } = false;
        public bool IsNameDropDownOpen { get; set; } = false;
        public string SelectedName { get; set; }
        public bool IsSearchingForNames { get; set; } = false;
        public bool IsSearchNameButtonEnabled { get; set; } = true;
        public bool IsResetNameButtonEnabled { get; set; } = true;


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
                    ValidateAsync();
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
                    ValidateAsync();
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

        public AddGameSourceTypes SourceTypes { get; set; } = AddGameSourceTypes.Vndb;

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
            SuggestedNamesCollection = new BindableCollection<string>();
        }

        public async Task Search()
        {
            if (string.IsNullOrEmpty(VnName) || string.IsNullOrWhiteSpace(VnName)) return;
            CanChangeVnName = false;
            IsSearchNameButtonEnabled = false;
            IsResetNameButtonEnabled = false;
            await ValidateAsync();
            if (VndbConnectionTest.VndbTcpSocketTest() == false)
            {
                LogManager.Logger.Warning("Could not connect to the Vndb API over SSL");
                await Task.Delay(3500);
            }
            try
            {
                if (VnName == null || VnName.Length < 2) return;
                using (Vndb client = new Vndb(true))
                {
                    SuggestedNamesCollection.Clear();
                    VndbResponse<VisualNovel> _vnNameList = null;
                    IsSearchingForNames = true;
                    _vnNameList = await client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(VnName), VndbFlags.Basic);
                    if(_vnNameList == null)
                    {
                        //handle error
                        return;
                    }
                    else if(_vnNameList.Count < 1)
                    {
                        //handle error
                        return;
                    }
                    List<string> nameList = IsJapaneseText(VnName) == true ? _vnNameList.Select(item => item.OriginalName).ToList() : _vnNameList.Select(item => item.Name).ToList();
                    SuggestedNamesCollection.AddRange(nameList.Where(x => !string.IsNullOrEmpty(x)).ToList());
                    IsNameDropDownOpen = true;
                    SelectedName = SuggestedNamesCollection.FirstOrDefault();
                }
                IsResetNameButtonEnabled = true;
                IsSearchingForNames = false;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex, "Failed to search VnName");
                IsSearchingForNames = false;
                throw;
            }
        }

        private bool IsJapaneseText(string text)
        {
            Regex regex = new Regex(@"/[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B/g");
            return regex.IsMatch(text);
        }

        public void ResetName()
        {
            IsNameDropDownOpen = false;
            SuggestedNamesCollection.Clear();
            CanChangeVnName = true;
            IsSearchNameButtonEnabled = true;
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
            ValidateAsync();
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
    public enum AddGameSourceTypes { NoSource, Vndb}

    public class AddGameViewModelValidator : AbstractValidator<AddGameViewModel>
    {
        public AddGameViewModelValidator()
        {

            RuleFor(x => x.VnId).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).When(x => x.IsNameChecked == false).WithMessage("Vndb ID must be greater than 0")
                .MustAsync(IsNotAboveMaxId).Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).When(x => x.IsNameChecked == false).WithMessage("ID entered is above maximum Vndb ID")
                .MustAsync(IsNotDeletedVn).Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).When(x => x.IsNameChecked == false).WithMessage("The Vndb ID entered has been removed or does not exist");

            When(x => x.IsNameChecked == true, () =>
            {
                RuleFor(x => x.CanChangeVnName).NotEqual(true).Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).WithMessage("A selection from the list of VN names is required");
                RuleFor(x => x.VnName).NotEmpty().Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).When(x => x.CanChangeVnName ==false).WithMessage("Vn Name cannot be empty");
            });


            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("Exe Path cannot be empty")
                .Must(ValidateFiles.EndsWithExe).WithMessage("Not a valid path to exe")
                .Must(ValidateFiles.ValidateExe).WithMessage("Not a valid Executable");


            RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().When(x => x.IsIconChecked == true).Unless(x => x.HideIconError == true).WithMessage("Icon path cannot be empty")
                .Must(ValidateFiles.EndsWithIcoOrExe).When(x => x.IsIconChecked == true).Unless(x => x.HideIconError == true).WithMessage("Not a valid path to icon");

            RuleFor(x => x.ExeArguments).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().When(x => x.IsArgsChecked == true && x.ExeArguments == "").Unless(x => x.HideArgumentsError == true).WithMessage("Arguments cannot be empty")
                .Must(AddGameMultiViewModelValidator.ContainsIllegalCharacters).When(x => x.IsArgsChecked == true && x.ExeArguments == "").Unless(x => x.HideArgumentsError == true).WithMessage("Illegal characters detected");

        }


        #region Validators

        private async Task<bool> IsNotDeletedVn(int inputid, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    uint vnid = Convert.ToUInt32(inputid); //17725 is deleted vn
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid));
                    if (response != null)
                    {
                        return response.Count > 0;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex, "check for deleted vn failed");
                return false;
            }
        }

        private async Task<bool> IsNotAboveMaxId(int id, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    RequestOptions ro = new RequestOptions { Reverse = true, Sort = "id",Count = 1 };
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                    if (response != null)
                    {
                        if (id < response.Items[0].Id) return true;
                        else return false;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex, "Could not check max vndb id", ex.Data);
                return false;
            }
        }

        private bool IsNotDuplicateVn(int id)
        {
            throw new NotImplementedException("Need to add support for LiteDb");
        }

        #endregion
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
