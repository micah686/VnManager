using Stylet;
using System;
using System.Collections.Generic;
using System.Text;
using AdysTech.CredentialManager;
using LiteDB;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.ModifyGame;

namespace VnManager.ViewModels.UserControls
{
    public class CategoryListViewModel:Screen
    {
        public BindableCollection<string> CategoryCollection { get; private set; }

        public CategoryListViewModel()
        {
            try
            {
                CategoryCollection = new BindableCollection<string>();
                CategoryCollection.Clear();
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return;
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
    }
}
