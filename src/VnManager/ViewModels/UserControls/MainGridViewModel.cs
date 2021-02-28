﻿// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
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
        /// <summary>
        /// The main grid of the application. This excludes the categoriesListView
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="category"></param>
        /// <param name="addGame"></param>
        /// <param name="gameGrid"></param>
        /// <param name="noGames"></param>
        public MainGridViewModel(IWindowManager windowManager, Func<CategoryListViewModel> category, Func<AddGameMainViewModel> addGame,
            Func<GameGridViewModel> gameGrid, Func<NoGamesViewModel> noGames)
        {
            _windowManager = windowManager;
            _addGameFactory = addGame;
            CategoryListPage = category();
            CheckGames(gameGrid, noGames);
        }

        /// <summary>
        /// Check how many games there are, and display the correct view accourdingly
        /// </summary>
        /// <param name="gameGrid"></param>
        /// <param name="noGames"></param>
        private void CheckGames(Func<GameGridViewModel> gameGrid, Func<NoGamesViewModel> noGames)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }

                int gameCount;
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
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
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to check games for GameGrid");
                SentrySdk.CaptureException(e);
            }
        }

        /// <summary>
        /// Add a game to the database
        /// <see cref="ShowAddGameDialog"/>
        /// </summary>
        public void ShowAddGameDialog()
        {
            try
            {
                _windowManager.ShowDialog(_addGameFactory());
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to show AddGameDialog");
                SentrySdk.CaptureException(e);
            }
        }
    }
}
