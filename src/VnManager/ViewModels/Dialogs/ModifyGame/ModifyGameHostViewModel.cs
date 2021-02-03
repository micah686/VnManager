// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Interfaces;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    
    public class ModifyGameHostViewModel: Conductor<Screen>.Collection.OneActive
    {
        public string WindowTitle { get; set; }
        public string GameTitle { get; set; }
        public bool BlockClosing { get; set; } = false;
        public bool EnableTabs { get; set; } = true;

        private UserDataGames _selectedGame;
        private readonly IWindowManager _windowManager;
        private readonly IModifyGamePathFactory _gamePath;
        private readonly IModifyGameCategoriesFactory _gameCategories;
        private readonly IModifyGameDeleteFactory _gameDelete;
        private readonly IModifyGameRepairFactory _gameRepair;
        public ModifyGameHostViewModel(IWindowManager windowManager,
            IModifyGamePathFactory gamePath, IModifyGameCategoriesFactory gameCategories, IModifyGameDeleteFactory gameDelete, IModifyGameRepairFactory gameRepair)
        {
            _windowManager = windowManager;
            
            _gamePath = gamePath;
            _gameCategories = gameCategories;
            _gameDelete = gameDelete;
            _gameRepair = gameRepair;
        }

        protected override void OnViewLoaded()
        {
            var gamePath = _gamePath.CreateModifyGamePath();
            var gameCategories = _gameCategories.CreateModifyGameCategories();
            var gameDelete = _gameDelete.CreateModifyGameDelete();
            var gameRepair = _gameRepair.CreateModifyGameRepair();

            gamePath.SelectedGame = _selectedGame;
            gameCategories.SelectedGame = _selectedGame;
            gameDelete.SelectedGame = _selectedGame;
            gameRepair.SelectedGame = _selectedGame;

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
            _selectedGame = game;
            SetTitle();
            
        }

        private void SetTitle()
        {
            switch (_selectedGame.SourceType)
            {
                case AddGameSourceType.Vndb:
                {
                    SetVndbTitle();

                    break;
                }
                case AddGameSourceType.NoSource:
                    WindowTitle =  $"{App.ResMan.GetString("Modify")} {_selectedGame.Title}";
                    GameTitle = _selectedGame.Title;
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
                .Where(x => x.VnId == _selectedGame.GameId.Value).FirstOrDefault();
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
