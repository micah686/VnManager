// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using Stylet;
using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.UserControls
{
    public class MainGridViewModel: Conductor<Screen>
    {

        public LastPlayedViewModel LastPlayedPage { get; set; }
        public CategoryListViewModel CategoryListPage { get; set; }
        public AddGameButtonViewModel AddGamePage { get; set; }


        public static MainGridViewModel Instance { get; private set; }

        public MainGridViewModel(Func<LastPlayedViewModel> lastPlayed, Func<CategoryListViewModel> category, Func<AddGameButtonViewModel> addGame, Func<GameGridViewModel> gameGrid)
        {
            Instance = this;

            LastPlayedPage = lastPlayed();
            CategoryListPage = category();
            AddGamePage = addGame();

            ActivateItem(gameGrid());
        }

        public sealed override void ActivateItem(Screen item)
        {
            base.ActivateItem(item);
        }
    }
}
