using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MvvmValidation;
using VisualNovelManagerv2.CustomClasses.Database;
using VisualNovelManagerv2.Design;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.Infrastructure;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class AddVnViewModel: ValidatableViewModelBase
    {        
        private readonly AddVnViewModelService _service;
        public RelayCommand GetFile { get; private set; }
        public ICommand ValidateCommand { get; private set; }
        public ObservableCollection<string> SuggestedNamesCollection { get; set; }

        public AddVnViewModel()
        {
            //for openFileDialog
            this.GetFile = new RelayCommand(() => Messenger.Default.Send(_service));
            _service = new AddVnViewModelService {FilePicked = this.FilePicked};
            //for mvvmValidation
            ValidateCommand = new RelayCommand(Validate);
            ConfigureValidationRules();
            Validator.ResultChanged += OnValidationResultChanged;
            //for VnName search box
            this.SuggestedNamesCollection = new ObservableCollection<string>();
            SourceIndex = 0;
        }

        #region Static Properties

        private int? _vnId;
        public int? VnId
        {
            get { return _vnId; }
            set
            {
                _vnId = value;
                RaisePropertyChanged(nameof(VnId));
                Validator.ValidateAsync(VnId);
            }
        }

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

        private string _vnName;
        public string VnName
        {
            get { return _vnName; }
            set
            {
                _vnName = value;
                RaisePropertyChanged(nameof(VnName));
                Validator.ValidateAsync(VnName);
            }
        }

        private bool _isNameChecked;
        public bool IsNameChecked
        {
            get { return _isNameChecked; }
            set
            {
                _isNameChecked = value;
                RaisePropertyChanged(nameof(IsNameChecked));                                
            }
        }

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

        private int _sourceIndex;
        public int SourceIndex
        {
            get { return _sourceIndex; }
            set
            {
                _sourceIndex = value;
                RaisePropertyChanged(nameof(SourceIndex));
            }
        }

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
        private void FilePicked()
        {
            this.FileName = _service.PickedFileName;
        }

        private void ConfigureValidationRules()
        {
            Validator.AddRequiredRule(() => VnId, "Vndb ID is required");
            Validator.AddRule(nameof(VnId),
                () => RuleResult.Assert(VnId >= 1, "Vndb ID must be at least 1"));
            Validator.AddRule(nameof(VnId),
                () => RuleResult.Assert(VnId <= GetDatabaseVnCount().Result, "Not a Valid Vndb ID"));


            Validator.AddRequiredRule(() => FileName, "Path to application is required");
            Validator.AddRule(nameof(FileName),
                () =>
                {
                    bool filepath = File.Exists(FileName);
                    string ext = Path.GetExtension(FileName) ?? string.Empty;
                    return RuleResult.Assert(filepath && ext.EndsWith(".exe"), "Not a valid file path");
                });
        }

        private static async Task<uint> GetDatabaseVnCount()
        {
	        using (Vndb client = new Vndb(true).WithClientDetails("VisualNovelManagerv2", "0.0.0"))
	        {
		        DatabaseStats stats = await client.GetDatabaseStatsAsync();
				client.Logout();
		        return stats.VisualNovels;
	        }
		}

        private void ValidateExe()
        {
            if (!File.Exists(FileName)) return;
            var twoBytes = new byte[2];
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

        public async void SearchName()
        {
            if(VnName == null || VnName.Length <= 2)return;
            if(IsRunning != false)return;
            IsRunning = true;
            using (Vndb client= new Vndb(true))
            {
                VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(VnName));
                //namelist gets a  list of english names if text input was english, or japanese names if input was japanese
                List<string> nameList = IsJapaneseText(VnName) == true ? response.Select(item => item.OriginalName).ToList() : response.Select(item => item.Name).ToList();
                foreach (string name in nameList)
                {
                    SuggestedNamesCollection.Add(name);
                }
                IsDropDownOpen = true;
                IsRunning = false;
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
            ValidateExe();
            await ValidateAsync();
            if (IsValid == true)
            {
                AddToDatabase atd = new AddToDatabase();
                atd.GetId(Convert.ToInt32(VnId), FileName);
                FileName = String.Empty;
                VnId = 0;
            }
            
        }

        private async Task ValidateAsync()
        {
            var result = await Validator.ValidateAllAsync();

            UpdateValidationSummary(result);
        }

        private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
        {
            if (!IsValid.GetValueOrDefault(true))
            {
                ValidationResult validationResult = Validator.GetResult();

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
