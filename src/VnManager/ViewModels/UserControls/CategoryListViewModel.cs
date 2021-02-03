// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using System;
using AdysTech.CredentialManager;
using LiteDB;
using VnManager.Events;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.UserControls
{
    public class CategoryListViewModel:Screen, IHandle<UpdateEvent>
    {
        public BindableCollection<string> CategoryCollection { get; private set; }

        public int SelectedIndex { get; set; } = 0;
        public static string SelectedCategory { get; set; } = "All";

        private readonly IEventAggregator _events;
        public CategoryListViewModel(IEventAggregator events)
        {
            _events = events;
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
            _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
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
