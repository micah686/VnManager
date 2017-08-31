using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using MvvmValidation;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.Infrastructure;
using ValidationResult = System.Windows.Controls.ValidationResult;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    /// <summary>
    ///Interface that implements the Close method
    /// </summary>
    public interface IClosable
    {
        void Close();
    }
    public class VnMainCategoryOptionsViewModel: ValidatableViewModelBase
    {

        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                AddCategoryText = null;
                RemoveCategoryText = null;
                CategoriesCollection.Clear();
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
        #endregion

        #region AddCategoryText
        private string _addCategoryText;
        public string AddCategoryText
        {
            get { return _addCategoryText; }
            set
            {
                _addCategoryText = value;
                if (value != null)
                {
                    _addCategoryText = value.Trim();
                }                
                RaisePropertyChanged(nameof(AddCategoryText));
            }
        }
        #endregion

        #region RemoveCategoryText
        private string _removeCategoryText;
        public string RemoveCategoryText
        {
            get { return _removeCategoryText; }
            set
            {
                _removeCategoryText = value;
                RaisePropertyChanged(nameof(RemoveCategoryText));
            }
        }
        #endregion

        #region BbCodeTextDone
        private string _bbCodeTextDone;
        public string BbCodeTextDone
        {
            get { return _bbCodeTextDone; }
            set
            {
                _bbCodeTextDone = value;
                RaisePropertyChanged(nameof(BbCodeTextDone));
            }
        }
        #endregion

        #region IsDropdownOpen
        private bool _isDropdownOpen;
        public bool IsDropdownOpen
        {
            get { return _isDropdownOpen; }
            set
            {
                _isDropdownOpen = value;
                RaisePropertyChanged(nameof(IsDropdownOpen));
            }
        }
        #endregion

        #region CategoriesCollection
        private RangeEnabledObservableCollection<string> _categoriesCollection= new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> CategoriesCollection
        {
            get { return _categoriesCollection; }
            set
            {
                _categoriesCollection = value;
                RaisePropertyChanged(nameof(CategoriesCollection));
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
        #endregion ValidationErrorsString
        public ICommand LoadCategoryListCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCategoryList);
        public ICommand SumbitCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<IClosable>(OnSubmitClicked);
        public ICommand CancelCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<IClosable>(OnCloselClicked);
        private bool _isValid = false;


        public VnMainCategoryOptionsViewModel()
        {
            
        }

        private void LoadCategoryList()
        {
            using (var context = new DatabaseContext())
            {
                List<string> categories = context.Categories.Where(x => x.CategoryName != "All").Select(x => x.CategoryName)
                    .ToList();
                if (categories.Count >0)
                {                    
                    CategoriesCollection.InsertRange(categories);
                }

            }
        }

        private bool CheckAddCategoryName()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    bool data = context.Categories.Any(x => x.CategoryName == AddCategoryText);
                    return data;
                }
            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }
            
        }

        private void RemoveCategory()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    //get the category element to remove
                    CategoryJunction data = context.CategoryJunction.FirstOrDefault(x =>
                        x.Category.CategoryName == RemoveCategoryText);
                    if (data != null)
                    {
                        context.CategoryJunction.Remove(data);
                        context.SaveChanges();

                        //reload category list
                        CategoriesCollection.Clear();
                        LoadCategoryList();
                    }                    
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private void AddCategory()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    Category data = new Category{CategoryName = AddCategoryText};
                    if (!string.IsNullOrEmpty(data.CategoryName))
                    {
                        context.Categories.Add(data);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private async void OnSubmitClicked(IClosable window)
        {
            try
            {
                Validate();
                if (AddCategoryText != null  && _isValid)
                {
                    AddCategory();
                    BbCodeTextDone = "[color=#0f0]Done[/color]";
                    await Task.Delay(1500);
                    BbCodeTextDone = null;
                    AddCategoryText = null;
                    RemoveCategoryText = null;

                }
                else if (RemoveCategoryText != null)
                {
                    RemoveCategory();
                    BbCodeTextDone = "[color=#0f0]Done[/color]";
                    await Task.Delay(1500);
                    BbCodeTextDone = null;
                    AddCategoryText = null;
                    RemoveCategoryText = null;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            } 
        }

        private void OnCloselClicked(IClosable window)
        {
            window?.Close();
        }





        private void ConfigureValidationRules()
        {
            Validator.AddRule(nameof(AddCategoryText),
                () => RuleResult.Assert(!string.IsNullOrEmpty(AddCategoryText), "Cannot be empty"));
            Validator.AddRule(nameof(AddCategoryText),
                () => RuleResult.Assert(CheckAddCategoryName() != true, "Category already exists"));
        }

        #region Validation Methods

        private async void Validate()
        {
            try
            {
                ConfigureValidationRules();
                Validator.ResultChanged += OnValidationResultChanged;
                await ValidateAsync();
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private async Task ValidateAsync()
        {
            MvvmValidation.ValidationResult result = await Validator.ValidateAllAsync();

            UpdateValidationSummary(result);
        }

        private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
        {
            if (!_isValid.Equals(true))
            {
                MvvmValidation.ValidationResult validationResult = Validator.GetResult();
                Debug.WriteLine(" validation updated: " + validationResult);
                UpdateValidationSummary(validationResult);
            }
        }

        private void UpdateValidationSummary(MvvmValidation.ValidationResult validationResult)
        {
            _isValid = validationResult.IsValid;
            ValidationErrorsString = validationResult.ToString();
        }

        #endregion
    }
}
