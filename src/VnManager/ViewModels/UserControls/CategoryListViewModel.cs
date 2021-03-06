// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
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
        /// <summary>
        /// Main view for CategoryListView
        /// </summary>
        /// <param name="events"></param>
        public CategoryListViewModel(IEventAggregator events)
        {
            _events = events;
            SetupEvents(events);
            ReloadCategories();
            
        }
        /// <summary>
        /// Subscribe to Refreshing Categories List
        /// </summary>
        /// <param name="eventAggregator"></param>
        private void SetupEvents(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this, EventChannels.RefreshCategoryList.ToString());
        }

        /// <summary>
        /// Reload the categories list
        /// </summary>
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
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'");
                var categoryData = db.GetCollection<UserDataCategories>(DbUserData.UserData_Categories.ToString()).FindAll();
                var categoryList = categoryData.Where(x => x.CategoryName != "All").OrderBy(x => x.CategoryName)
                    .Select(x => x.CategoryName).ToList();
                categoryList.Insert(0, "All");
                CategoryCollection.AddRange(categoryList);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "An Error happened in CategoryListViewModel");
                SentrySdk.CaptureException(e);
            }
        }
        
        
        
        /// <summary>
        /// Refresh the current Game Grid when a selected category changes
        /// </summary>
        public void SelectionChanged()
        {
            _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
        }

        public void Handle(UpdateEvent message)
        {
            if (message != null && message.ShouldUpdate)
            {
                ReloadCategories();
                SelectedIndex = 0;
            }
            
        }
    }
}
