// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.UserControls
{
    public class MainGridViewModel: Conductor<Screen>
    {
        public CategoryListViewModel CategoryListPage { get; set; }

        private readonly IWindowManager _windowManager;
        private readonly Func<AddGameMainViewModel> _addGameFactory;
        public MainGridViewModel(IWindowManager windowManager, Func<CategoryListViewModel> category, Func<AddGameMainViewModel> addGame,
            Func<GameGridViewModel> gameGrid, Func<NoGamesViewModel> noGames)
        {
            _windowManager = windowManager;
            _addGameFactory = addGame;
            CategoryListPage = category();
            CheckGames(gameGrid, noGames);
        }

        private void CheckGames(Func<GameGridViewModel> gameGrid, Func<NoGamesViewModel> noGames)
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }

            int gameCount;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                gameCount = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Count();
                
            }

            if (gameCount < 1)
            {
                ActivateItem(noGames());
            }
            else
            {
                ActivateItem(gameGrid());
            }
        }

        public void ShowAddGameDialog()
        {
            _windowManager.ShowDialog(_addGameFactory());
        }
    }
}
