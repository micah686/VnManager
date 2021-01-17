using Stylet;
using System;
using AdysTech.CredentialManager;
using LiteDB;
using StyletIoC;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.UserControls
{
    public class CategoryListViewModel:Screen
    {
        public BindableCollection<string> CategoryCollection { get; private set; }

        public int SelectedIndex { get; set; } = 0;
        public static string SelectedCategory { get; set; } = "All";

        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public CategoryListViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            try
            {
                CategoryCollection = new BindableCollection<string>();
                CategoryCollection.Clear();
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                var categoryArray = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString()).Query()
                    .Select(x => x.CategoryName).ToArray();
                CategoryCollection.AddRange(categoryArray);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "An Error happened in CategoryListViewModel");
                throw;
            }
            
        }

        public void SelectionChanged()
        {
            var vm = _container.Get<GameGridViewModel>();
            
            MainGridViewModel.Instance.ActivateItem(vm);
        }
    }
}
