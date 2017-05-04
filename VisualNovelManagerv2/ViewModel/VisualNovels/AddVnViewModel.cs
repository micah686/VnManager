using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MvvmValidation;
using VisualNovelManagerv2.Design;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.Infrastructure;
using VndbSharp;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class AddVnViewModel: ValidatableViewModelBase
    {
        
        private readonly AddVnViewModelService _service;
        public RelayCommand GetFile { get; private set; }
        public ICommand ValidateCommand { get; private set; }

        public AddVnViewModel()
        {
            //for openFileDialog
            this.GetFile = new RelayCommand(() => Messenger.Default.Send(_service));
            _service = new AddVnViewModelService {FilePicked = this.FilePicked};
            //for mvvmValidation
            ValidateCommand = new RelayCommand(Validate);
            ConfigureValidationRules();
            Validator.ResultChanged += OnValidationResultChanged;
        }

        #region Static Properties

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

        private uint _vnId;
        public uint VnId
        {
            get { return _vnId; }
            set
            {
                _vnId = value;
                RaisePropertyChanged(nameof(VnId));
                Validator.ValidateAsync(VnId);
            }
        }

        #endregion

        private void FilePicked()
        {
            this.FileName = _service.PickedFileName;
        }

        private void ConfigureValidationRules()
        {
            Validator.AddRequiredRule(()=> VnId, "Vndb ID is required");
            Validator.AddRule(nameof(VnId),
                () => RuleResult.Assert(VnId >= 1, "Vndb ID must be at leat 1"));
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

        private async Task<uint> GetDatabaseVnCount()
        {
	        using (Vndb _client = new Vndb(true).WithClientDetails("VisualNovelManagerv2", "0.0.0"))
	        {
		        var stats = await _client.GetDatabaseStatsAsync();
				_client.Logout();
		        return stats.VisualNovels;
	        }
		}

        #region Validation Methods
        private async void Validate()
        {
            await ValidateAsync();
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
}
