using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MvvmValidation;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.CustomClasses.Vndb;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.Infrastructure;
using VisualNovelManagerv2.Pages.VisualNovels;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels.AddVn
{
    public partial class AddVnViewModel: ValidatableViewModelBase
    {                        
        public AddVnViewModel()
        {
            //for openFileDialog
            this.GetFile = new RelayCommand(() => Messenger.Default.Send(_exeService));
            this.GetIcon = new RelayCommand(() => AddVisualNovel.IconMessenger.Send(_iconService));
            _exeService = new AddVnViewModelService {FilePicked = this.FilePicked};
            _iconService = new AddVnViewModelService { FilePicked = this.IconPicked};
            //for mvvmValidation
            this.SuggestedNamesCollection = new ObservableCollection<string>();
            DropdownIndex = 0;
        }
        
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
                Validator.AddRequiredRule(() => InputVnId, "Vndb ID is required");
                Validator.AddRule(nameof(InputVnId),
                    () => RuleResult.Assert(InputVnId >= 1, "Vndb ID must be at least 1"));
                Validator.AddRule(nameof(InputVnId),
                    () => RuleResult.Assert(InputVnId <= IsAboveMaxId().Result, "Not a Valid Vndb ID"));
                Validator.AddRule(nameof(InputVnId),
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
                using (Vndb client = new Vndb(true))
                {
                    RequestOptions ro = new RequestOptions
                    {
                        Reverse = true,
                        Sort = "id",
                        Count = 1
                    };
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                    return response.Items[0].Id;
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
                using (Vndb client = new Vndb(true))
                {
                    uint vnid = Convert.ToUInt32(InputVnId);
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
                IsUserInputEnabled = false;
                ValidateExe();
                //set validation rules here, so they are are checked on submit
                ConfigureValidationRules();
                Validator.ResultChanged += OnValidationResultChanged;
                await ValidateAsync();
                if (IsValid == true)
                {                    
                    if (!string.IsNullOrEmpty(VnName))
                    {                        
                        _vnid = _vnNameList.Items[DropdownIndex].Id;
                        GetData();
                    }
                    else
                    {
                        _vnid = Convert.ToUInt32(InputVnId);
                        GetData();
                    }
                }
                else
                {
                    IsUserInputEnabled = true;
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
