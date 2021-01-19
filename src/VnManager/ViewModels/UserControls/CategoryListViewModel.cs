using Stylet;
using System;
using AdysTech.CredentialManager;
using LiteDB;
using StyletIoC;
using VnManager.Events;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.UserControls
{
    public class CategoryListViewModel:Screen, IHandle<UpdateEvent>
    {
        public BindableCollection<string> CategoryCollection { get; private set; }

        public int SelectedIndex { get; set; } = 0;
        public static string SelectedCategory { get; set; } = "All";

        private readonly IContainer _container;
        public CategoryListViewModel(IContainer container, IEventAggregator events)
        {
            _container = container;
            SetupEvents(events);
            ReloadCategories();
            
        }

        private void SetupEvents(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this, EventChannels.RefreshCategoryList.ToString());
        }

        private void ReloadCategories()
        {
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

        public void Handle(UpdateEvent message)
        {
            if (message != null && message.ShouldUpdate)
            {
                ReloadCategories();
            }
            
        }
    }
}
