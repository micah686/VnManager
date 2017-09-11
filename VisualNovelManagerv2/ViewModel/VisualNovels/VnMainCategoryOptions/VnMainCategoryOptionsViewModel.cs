using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmValidation;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.Infrastructure;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMainCategoryOptions
{
    /// <summary>
    ///Interface that implements the Close method
    /// </summary>
    public interface IClosable
    {
        void Close();
    }
    public partial class VnMainCategoryOptionsViewModel: ValidatableViewModelBase
    {
        
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
