// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using Stylet;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.UserControls
{
    public class AddGameButtonViewModel: Conductor<Screen>
    {
        private readonly IWindowManager _windowManager;
        private readonly Func<AddGameMainViewModel> _addGameFactory;
        public AddGameButtonViewModel(IWindowManager windowManager, Func<AddGameMainViewModel> addGame) 
        {
            _windowManager = windowManager;
            _addGameFactory = addGame;
        }

        public void ShowAddGameDialog()
        {
            _windowManager.ShowDialog(_addGameFactory());
            
        }

        
    }

}
