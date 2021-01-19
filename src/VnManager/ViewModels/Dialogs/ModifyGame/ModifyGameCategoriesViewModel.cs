using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using Stylet;
using VnManager.Events;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameCategoriesViewModel: Screen
    {
        #region Properties
        //Add Category
        public BindableCollection<string> AddCategoriesCollection { get; } = new BindableCollection<string>();
        public bool IsAddCategoriesEnabled { get; set; }
        public int SelectedAddIndex { get; set; }
        public string SelectedAddValue { get; set; }
        public string AddCategoryMessage { get; set; }
        //Remove Category
        public BindableCollection<string> RemoveCategoriesCollection { get; } = new BindableCollection<string>();
        public bool IsRemoveCategoriesEnabled { get; set; }
        public int SelectedRemoveIndex { get; set; }
        public string SelectedRemoveValue { get; set; }
        public string RemoveCategoryMessage { get; set; }

        //Create Category
        public string NewCategoryValue { get; set; }
        //Delete Category
        public BindableCollection<string> DeleteCategoriesCollection { get; } = new BindableCollection<string>();
        public string DeleteCategorySelectedValue { get; set; }
        public int DeleteCategoryIndex { get; set; }
        public bool DeleteCategoryEnabled { get; set; }

        #endregion

        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        public ModifyGameCategoriesViewModel(IWindowManager windowManager, IEventAggregator events, IModelValidator<ModifyGameCategoriesViewModel> validator) : base(validator)
        {
            _windowManager = windowManager;
            _events = events;
            DisplayName = App.ResMan.GetString("Categories");

        }

        protected override void OnViewLoaded()
        {
            IsRemoveCategoriesEnabled = true;
            FillCategories();
        }


        private void FillCategories()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserCategories = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString());

            var categoriesExceptAll = dbUserCategories.Query().Where(x => x.CategoryName != "All").Select(x => x.CategoryName).ToList();
            

            AddCategoryMessage = $"{App.ResMan.GetString("AddCategoryTo")} {ModifyGameHostViewModel.GameTitle}";
            RemoveCategoryMessage = $"{App.ResMan.GetString("RemoveCategoryFrom")} {ModifyGameHostViewModel.GameTitle}";

            AddCategoriesCollection.Clear();
            RemoveCategoriesCollection.Clear();
            DeleteCategoriesCollection.Clear();

            if(ModifyGameHostViewModel.SelectedGame.Categories == null)
            {
                return;
            }
            
            FillAddCategory(categoriesExceptAll);
            FillRemoveCategory(categoriesExceptAll);
            FillDeleteCategory(categoriesExceptAll);
            
            DeleteCategoryIndex = 0;

        }

        #region AddCategory
        private void FillAddCategory(List<string> exceptAllList)
        {
            if (exceptAllList.Count > 0 && ModifyGameHostViewModel.SelectedGame.Categories.Count > 0)
            {
                var validAdd = exceptAllList.Except(ModifyGameHostViewModel.SelectedGame.Categories).ToList();
                validAdd.Remove("All");
                if (validAdd.Count >0)
                {
                    AddCategoriesCollection.AddRange(validAdd);
                    IsAddCategoriesEnabled = true;
                    SelectedAddIndex = 0;
                }
                else
                {
                    IsAddCategoriesEnabled = false;
                }
                
            }
            else
            {
                AddCategoriesCollection.Clear();
                IsAddCategoriesEnabled = false;
                SelectedAddValue = string.Empty;
            }
        }

        public void SaveAddCategory()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                var entry = ModifyGameHostViewModel.SelectedGame;
                if (entry.Categories.Count > 0)
                {
                    ModifyGameHostViewModel.SelectedGame.Categories.Add(SelectedAddValue);
                    ModifyGameHostViewModel.SelectedGame.Categories = ModifyGameHostViewModel.SelectedGame.Categories
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                }

                dbUserData.Update(ModifyGameHostViewModel.SelectedGame);
            }

            FillCategories();

        }

        #endregion

        #region RemoveCategory
        private void FillRemoveCategory(List<string> exceptAllList)
        {
            if (exceptAllList.Count > 0 && ModifyGameHostViewModel.SelectedGame.Categories.Count > 1)
            {
                var validRemove = exceptAllList.Intersect(ModifyGameHostViewModel.SelectedGame.Categories).ToList();
                validRemove.Remove("All");
                if (validRemove.Count > 0)
                {
                    RemoveCategoriesCollection.AddRange(validRemove);
                    IsRemoveCategoriesEnabled = true;
                    SelectedRemoveIndex = 0;
                }
                else
                {
                    IsRemoveCategoriesEnabled = false;
                }
                
            }
            else
            {
                RemoveCategoriesCollection.Clear();
                IsRemoveCategoriesEnabled = false;
                SelectedRemoveValue = string.Empty;
            }
        }

        public void SaveRemoveCategory()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                var entry = ModifyGameHostViewModel.SelectedGame;
                if (entry.Categories.Count > 0)
                {
                    ModifyGameHostViewModel.SelectedGame.Categories.Remove(SelectedRemoveValue);
                    ModifyGameHostViewModel.SelectedGame.Categories = ModifyGameHostViewModel.SelectedGame.Categories
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                }

                dbUserData.Update(ModifyGameHostViewModel.SelectedGame);
            }
            FillCategories();
        }
        #endregion

        private void FillDeleteCategory(List<string> categoriesExceptAll)
        {
            DeleteCategoriesCollection.AddRange(categoriesExceptAll);
            if (DeleteCategoriesCollection.Count > 0)
            {
                DeleteCategoryEnabled = true;
            }
            else
            {
                DeleteCategoryEnabled = false;
            }
            
        }

        public async Task CreateNewCategory()
        {
            bool result = await ValidateAsync();
            if (result)
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    var dbUserCategories = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString());
                    var newCategory = new UserDataCategories { CategoryName = NewCategoryValue };
                    dbUserCategories.Insert(newCategory);
                }
                _windowManager.ShowMessageBox($"{App.ResMan.GetString("CreatedCategory")} {NewCategoryValue}", App.ResMan.GetString("CreatedCategory"));
                _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshCategoryList.ToString());
                FillCategories();
                NewCategoryValue = string.Empty;
            }
        }

        public void DeleteCategory()
        {
            var result = _windowManager.ShowMessageBox($"{App.ResMan.GetString("ConfirmDeleteCategoryMsg")} {NewCategoryValue}", App.ResMan.GetString("ConfirmDeleteCategory"), MessageBoxButton.YesNo);
            if(result == MessageBoxResult.No)
            {
                return;
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserCategories = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString());
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                List<UserDataGames> newGamesList = new List<UserDataGames>();
                foreach (var userData in dbUserData.Query().ToArray())
                {
                    if (userData.Categories != null && userData.Categories.Count >1 && DeleteCategorySelectedValue != "All")
                    {
                        userData.Categories.RemoveAll(x => x == DeleteCategorySelectedValue);
                        newGamesList.Add(userData);
                    }
                }
                
                dbUserCategories.DeleteMany(c => c.CategoryName == DeleteCategorySelectedValue);
            }

            _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshCategoryList.ToString());
            FillCategories();
        }

    }

    public class ModifyGameCategoriesViewModelValidator : AbstractValidator<ModifyGameCategoriesViewModel>
    {
        private const int MaxCategories = 100;
        private const string AllCategory = "All";
        public ModifyGameCategoriesViewModelValidator()
        {
            RuleFor(x => x.NewCategoryValue).Cascade(CascadeMode.Stop)
                .Must(IsValidName).WithMessage(App.ResMan.GetString("ValidationAlphanumericSpace"))
                .Must(IsNewCategory).WithMessage(App.ResMan.GetString("ValidationCategoryAlreadyExists"))
                .Must(IsNotMaxCategoriesCount).WithMessage($"{App.ResMan.GetString("ValidationAboveMaxCategories")} ({MaxCategories})");
        }

        private bool IsValidName(string input)
        {
            const int minLetters = 2;
            if (input == null || input == AllCategory)
            {
                return false;
            }
            var validCharacters = input.All(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x));
            var minimumLetters = input.Count(char.IsLetter);
            return validCharacters && minimumLetters > minLetters;
        }

        private bool IsNewCategory(string input)
        {
            if (input == null || input == AllCategory)
            {
                return false;
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return false;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var userCategories = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString())
                .Query().ToArray();
            if (userCategories != null && userCategories.Any(x => x.CategoryName != input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsNotMaxCategoriesCount(string input)
        {
            if (input == null || input == AllCategory)
            {
                return false;
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return false;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var userCategories = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString()).Query()
                .ToArray();
            if (userCategories != null && userCategories.Length <= MaxCategories)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
