// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Windows;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameHostViewModel: Conductor<Screen>.Collection.OneActive
    {
        internal UserDataGames SelectedGame { get; private set; }
        public string WindowTitle { get; set; }
        public string GameTitle { get; set; }
        public bool BlockClosing { get; set; } = false;
        public bool EnableTabs { get; set; } = true;

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public ModifyGameHostViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            var gamePath = _container.Get<Func<ModifyGamePathViewModel>>().Invoke();
            var gameCategories = _container.Get<Func<ModifyGameCategoriesViewModel>>().Invoke();
            var gameDelete = _container.Get<Func<ModifyGameDeleteViewModel>>().Invoke();
            var gameRepair = _container.Get<Func<ModifyGameRepairViewModel>>().Invoke();
            Items.Add(gamePath);
            Items.Add(gameCategories);
            Items.Add(gameDelete);
            Items.Add(gameRepair);
            
            ActivateItem(gamePath);
            
        }

        public sealed override void ActivateItem(Screen item)
        {
            base.ActivateItem(item);
        }


        public override Task<bool> CanCloseAsync()
        {
            if (BlockClosing)
            {
                _windowManager.ShowMessageBox(App.ResMan.GetString("ClosingDisabledMessage"), App.ResMan.GetString("ClosingDisabledTitle"), MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return Task.FromResult(false);

            }
            return base.CanCloseAsync();
        }

        internal void SetSelectedGame(UserDataGames game)
        {
            SelectedGame = game;
            SetTitle();
            
        }

        private void SetTitle()
        {
            switch (SelectedGame.SourceType)
            {
                case AddGameSourceType.Vndb:
                {
                    SetVndbTitle();

                    break;
                }
                case AddGameSourceType.NoSource:
                    WindowTitle =  $"{App.ResMan.GetString("Modify")} {SelectedGame.Title}";
                    GameTitle = SelectedGame.Title;
                    break;
                default:
                    //do nothing
                    break;
            }
        }

        private void SetVndbTitle()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query()
                .Where(x => x.VnId == SelectedGame.GameId.Value).FirstOrDefault();
            if (dbUserData != null)
            {
                WindowTitle = $"{App.ResMan.GetString("Modify")} {dbUserData.Title}";
                GameTitle = dbUserData.Title;
            }
        }

        public void LockControls()
        {
            EnableTabs = false;
            BlockClosing = true;
        }
        public void UnlockControls()
        {
            EnableTabs = true;
            BlockClosing = false;
        }
    }
}
