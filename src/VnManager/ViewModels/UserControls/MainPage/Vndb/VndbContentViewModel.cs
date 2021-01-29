// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Stylet;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbContentViewModel: Conductor<Screen>.Collection.OneActive
    {

        internal static int VnId { get; private set; }

        internal static UserDataGames SelectedGame { get; private set; }
        

        internal bool IsGameRunning { get; set; }
        internal List<Process> ProcessList { get; set; } = new List<Process>();
        internal Stopwatch GameStopwatch { get; set; } = new Stopwatch();

        private readonly IWindowManager _windowManager;
        public VndbContentViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            var vInfo = new VndbInfoViewModel { DisplayName = App.ResMan.GetString("Main") };
            var vChar = new VndbCharactersViewModel { DisplayName = App.ResMan.GetString("Characters") };
            var vScreen = new VndbScreensViewModel { DisplayName = App.ResMan.GetString("Screenshots") };

            Items.Add(vInfo);
            Items.Add(vChar);
            Items.Add(vScreen);
        }

        public override Task<bool> CanCloseAsync()
        {
            if (IsGameRunning)
            {
                _windowManager.ShowMessageBox(App.ResMan.GetString("ClosingDisabledMessage"), App.ResMan.GetString("ClosingDisabledTitle"), MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return Task.FromResult(false);

            }
            return base.CanCloseAsync();
        }

        internal static void SetSelectedGame(UserDataGames game)
        {
            SelectedGame = game;
            VnId = SelectedGame.GameId;
        }
        

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            SelectedGame = new UserDataGames();
        }
    }

    
}
