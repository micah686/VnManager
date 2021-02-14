// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Sentry;
using Stylet;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbContentViewModel: Conductor<Screen>.Collection.OneActive
    {

        internal static int VnId { get; private set; }

        internal static UserDataGames SelectedGame { get; set; }
        

        internal bool IsGameRunning { get; set; }
        internal List<Process> ProcessList { get; set; } = new List<Process>();
        internal Stopwatch GameStopwatch { get; set; } = new Stopwatch();

        private readonly IWindowManager _windowManager;
        private readonly INavigationController _navigationController;
        /// <summary>
        /// Main "Host" for Vndb content screens
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="navigationController"></param>
        public VndbContentViewModel(IWindowManager windowManager, INavigationController navigationController)
        {
            try
            {
                _windowManager = windowManager;
                _navigationController = navigationController;
                var vInfo = new VndbInfoViewModel { DisplayName = App.ResMan.GetString("Main") };
                var vChar = new VndbCharactersViewModel { DisplayName = App.ResMan.GetString("Characters") };
                var vScreen = new VndbScreensViewModel { DisplayName = App.ResMan.GetString("Screenshots") };

                Items.Add(vInfo);
                Items.Add(vChar);
                Items.Add(vScreen);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to create VndbContentHost");
                SentrySdk.CaptureException(e);
                throw;
            }
        }

        /// <summary>
        /// Checks if a game is running. If so, prevent exiting the VndbContent VM
        /// </summary>
        /// <returns></returns>
        public override Task<bool> CanCloseAsync()
        {
            if (IsGameRunning)
            {
                _windowManager.ShowMessageBox(App.ResMan.GetString("ClosingDisabledGameMessage"), App.ResMan.GetString("ClosingDisabledGameTitle"), MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return Task.FromResult(false);

            }
            return base.CanCloseAsync();
        }

        /// <summary>
        /// Sets when the currently selected game should be
        /// </summary>
        /// <param name="game"></param>
        internal void SetSelectedGame(UserDataGames game)
        {
            SelectedGame = game;
            VnId = SelectedGame.GameId;
        }
        
        /// <summary>
        /// Closes the Content Host
        /// <see cref="CloseClick"/>
        /// </summary>
        public void CloseClick()
        {
            _navigationController.NavigateToMainGrid();
            SelectedGame = new UserDataGames();
        }
    }

    
}
