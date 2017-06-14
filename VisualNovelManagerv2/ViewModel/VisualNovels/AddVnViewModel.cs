using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MvvmValidation;
using Newtonsoft.Json;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.Database;
using VisualNovelManagerv2.Design;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.Infrastructure;
using VisualNovelManagerv2.Pages.VisualNovels;
using VndbSharp;
using VndbSharp.Interfaces;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class AddVnViewModel: ValidatableViewModelBase
    {        
        private readonly AddVnViewModelService _exeService;
        private readonly AddVnViewModelService _iconService;
        public RelayCommand GetFile { get; private set; }
        public RelayCommand GetIcon { get; private set; }
        public ICommand ValidateCommand { get; private set; }
        public ICommand SearchNamesCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(SearchName);
        public ObservableCollection<string> SuggestedNamesCollection { get; set; }
        public static uint _maxVnId;
        public static uint _selectedVnId;
        public static VndbResponse<VisualNovel> _vnNameList;

        public AddVnViewModel()
        {
            //for openFileDialog
            this.GetFile = new RelayCommand(() => Messenger.Default.Send(_exeService));
            this.GetIcon = new RelayCommand(() => AddVisualNovel.IconMessenger.Send(_iconService));
            _exeService = new AddVnViewModelService {FilePicked = this.FilePicked};
            _iconService = new AddVnViewModelService { FilePicked = this.IconPicked};
            //for mvvmValidation
            ValidateCommand = new RelayCommand(Validate);
            this.SuggestedNamesCollection = new ObservableCollection<string>();
            DropdownIndex = 0;
        }

        #region Static Properties

        #region VnId
        private int? _vnId;
        public int? VnId
        {
            get { return _vnId; }
            set
            {
                _vnId = value;
                RaisePropertyChanged(nameof(VnId));
                if (VnId == null) return;
                Validator.ValidateAsync(VnId);
            }
        }
        #endregion

        #region FileName
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged(nameof(FileName));
                Validator.ValidateAsync(FileName);

            }
        }
        #endregion

        #region IconName
        private string _iconName;
        public string IconName
        {
            get { return _iconName; }
            set
            {
                _iconName = value;
                RaisePropertyChanged(nameof(IconName));
                Validator.ValidateAsync(IconName);
            }
        }
        #endregion

        #region VnName
        private string _vnName;
        public string VnName
        {
            get { return _vnName; }
            set
            {
                _vnName = value;
                RaisePropertyChanged(nameof(VnName));
                if (VnName == null) return;
                Validator.ValidateAsync(VnName);
            }
        }
        #endregion

        public BitmapImage SearchImage => new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\assorted\search.png"));

        #region IsNameChecked
        private bool _isNameChecked;
        public bool IsNameChecked
        {
            get { return _isNameChecked; }
            set
            {
                _isNameChecked = value;
                VnName = null;
                VnId = null;
                RaisePropertyChanged(nameof(IsNameChecked));
            }
        }
        #endregion

        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
        #endregion

        #region IsRunning
        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                RaisePropertyChanged(nameof(IsRunning));
            }
        }
        #endregion

        #region IsDropDownOpen
        private bool _isDropDownOpen;
        public bool IsDropDownOpen
        {
            get { return _isDropDownOpen; }
            set
            {
                _isDropDownOpen = value;
                RaisePropertyChanged(nameof(IsDropDownOpen));
            }
        }
        #endregion

        #region DropdownIndex
        private int _dropdownIndex;
        public int DropdownIndex
        {
            get { return _dropdownIndex; }
            set
            {
                _dropdownIndex = value;
                RaisePropertyChanged(nameof(DropdownIndex));
            }
        }
        #endregion

        #region IsIconEnabled
        private bool _isIconEnabled;
        public bool IsIconEnabled
        {
            get { return _isIconEnabled; }
            set
            {
                _isIconEnabled = value;
                RaisePropertyChanged(nameof(IsIconEnabled));
            }
        }
        #endregion

        #region IsValid
        private bool? _isValid;
        public bool? IsValid
        {
            get { return _isValid; }
            private set
            {
                _isValid = value;
                RaisePropertyChanged(nameof(IsValid));
            }
        }
        #endregion

        #region ValidationErrorsString
        private string _validationErrorsString;
        public string ValidationErrorsString
        {
            get { return _validationErrorsString; }
            private set
            {
                _validationErrorsString = value;
                RaisePropertyChanged(nameof(ValidationErrorsString));
            }
        }
        #endregion

        #endregion
        private void FilePicked()
        {
            this.FileName = _exeService.PickedFileName;
        }

        private void IconPicked()
        {
            IconName = _iconService.PickedFileName;
        }

        private void ConfigureValidationRules()
        {
            if (IsNameChecked != true)
            {
                Validator.AddRequiredRule(() => VnId, "Vndb ID is required");
                Validator.AddRule(nameof(VnId),
                    () => RuleResult.Assert(VnId >= 1, "Vndb ID must be at least 1"));
                Validator.AddRule(nameof(VnId),
                    () => RuleResult.Assert(VnId <= IsAboveMaxId().Result, "Not a Valid Vndb ID"));
                Validator.AddRule(nameof(VnId),
                    () => RuleResult.Assert(IsDeletedVn().Result != true, "This Vndb ID has been removed"));
            }

            Validator.AddRequiredRule(() => FileName, "Path to application is required");
            Validator.AddRule(nameof(FileName),
                () =>
                {
                    bool filepath = File.Exists(FileName);
                    string ext = Path.GetExtension(FileName) ?? string.Empty;
                    return RuleResult.Assert(filepath && ext.EndsWith(".exe"), "Not a valid file path");
                });

            if (IsIconEnabled == true)
            {
                Validator.AddRule(nameof(IconName),
                    () =>
                    {
                        bool filepath = File.Exists(IconName);
                        string ext = Path.GetExtension(IconName) ?? string.Empty;
                        return RuleResult.Assert(filepath && ext.EndsWith(".ico"), "Not a valid file path");
                    });
            }
            
        }

        private async Task<uint> IsAboveMaxId()
        {
            try
            {
                using (Vndb client = new Vndb(true).WithClientDetails("VisualNovelManagerv2", "0.0.0"))
                {
                    RequestOptions ro = new RequestOptions
                    {
                        Reverse = true,
                        Sort = "id",
                        Count = 1
                    };
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                    _maxVnId = response.Items[0].Id;
                    client.Logout();
                    return _maxVnId;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        private async Task<bool> IsDeletedVn()
        {
            try
            {
                using (Vndb client = new Vndb(true).WithClientDetails("VisualNovelManagerv2", "0.0.0"))
                {
                    uint vnid = Convert.ToUInt32(VnId);
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid));

                    client.Logout();
                    return response.Count < 1;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        private void ValidateExe()
        {
            try
            {
                if (!File.Exists(FileName)) return;
                byte[] twoBytes = new byte[2];
                using (FileStream fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileStream.Read(twoBytes, 0, 2);
                }
                switch (Encoding.UTF8.GetString(twoBytes))
                {
                    case "MZ":
                        break;
                    case "ZM":
                        break;
                    default:
                        Validator.AddRule(nameof(FileName),
                            () => RuleResult.Invalid("This application is invalid"));
                        break;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        public async void SearchName()
        {
            try
            {
                if (VnName == null || VnName.Length <= 2) return;
                if (IsRunning != false) return;
                IsRunning = true;
                using (Vndb client = new Vndb(true))
                {
                    SuggestedNamesCollection.Clear();
                    _vnNameList = null;
                    _vnNameList = await client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(VnName));
                    //namelist gets a  list of english names if text input was english, or japanese names if input was japanese
                    List<string> nameList = IsJapaneseText(VnName) == true ? _vnNameList.Select(item => item.OriginalName).ToList() : _vnNameList.Select(item => item.Name).ToList();
                    foreach (string name in nameList)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            SuggestedNamesCollection.Add(name);
                        }
                    }
                    IsDropDownOpen = true;
                    IsRunning = false;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        private bool IsJapaneseText(string text)
        {
            Regex regex = new Regex(@"/[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B/g");
            return regex.IsMatch(text);
        }

        #region Validation Methods
        private async void Validate()
        {
            try
            {
                ValidateExe();
                //set validation rules here, so they are are checked on submit
                ConfigureValidationRules();
                Validator.ResultChanged += OnValidationResultChanged;
                await ValidateAsync();
                if (IsValid == true)
                {
                    if (VnName != null)
                    {
                        _selectedVnId = _vnNameList.Items[DropdownIndex].Id;
                        AddToDatabase atd = new AddToDatabase();
                        atd.GetId(Convert.ToInt32(_selectedVnId), FileName, IconName);
                        FileName = String.Empty;
                        VnId = 0;
                        VnName = string.Empty;
                        _selectedVnId = 0;
                    }
                    else
                    {
                        AddToDatabase atd = new AddToDatabase();
                        atd.GetId(Convert.ToInt32(VnId), FileName, IconName);
                        FileName = String.Empty;
                        VnId = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }                        
        }

        private async Task ValidateAsync()
        {
            ValidationResult result = await Validator.ValidateAllAsync();

            UpdateValidationSummary(result);
        }

        private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
        {
            if (!IsValid.GetValueOrDefault(true))
            {
                ValidationResult validationResult = Validator.GetResult();
                Debug.WriteLine(" validation updated: "+ validationResult);
                UpdateValidationSummary(validationResult);
            }
        }

        private void UpdateValidationSummary(ValidationResult validationResult)
        {
            IsValid = validationResult.IsValid;
            ValidationErrorsString = validationResult.ToString();
        }
        #endregion
    }

    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
                return "Vn Name";
            else
                return "Vn ID";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
