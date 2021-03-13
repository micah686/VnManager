// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using Sentry;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db.User;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.UserControls.MainPage.NoSource;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

namespace VnManager
{
    public interface INavigationController
    {
        void NavigateToMainGrid();
        void NavigateVndbHost(UserDataGames selectedGame);
        void NavigateToNoSource(UserDataGames selectedGame);
    }

    public interface INavigationControllerDelegate
    {
        void NavigateTo(IScreen screen);
    }

    public class NavigationController : INavigationController
    {
        private readonly Func<MainGridViewModel> _mainGridFactory;
        private readonly Func<VndbContentViewModel> _vndbHostFactory;
        private readonly Func<NoSourceMainViewModel> _noSourceFactory;
        public INavigationControllerDelegate Delegate { get; set; }

        /// <summary>
        /// Main navigation controller for changing the main screen
        /// </summary>
        /// <param name="mainGrid"></param>
        /// <param name="vndbHost"></param>
        /// <param name="noSource"></param>
        public NavigationController(Func<MainGridViewModel> mainGrid, Func<VndbContentViewModel> vndbHost, Func<NoSourceMainViewModel> noSource)
        {
            try
            {
                this._mainGridFactory = mainGrid;
                this._vndbHostFactory = vndbHost;
                this._noSourceFactory = noSource;
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to init NavigationController");
                SentryHelper.SendException(e, null, SentryLevel.Error);
                throw;
            }
            
        }

        /// <summary>
        /// Change the view to the MainGrid
        /// </summary>
        public void NavigateToMainGrid()
        {
            this.Delegate?.NavigateTo(this._mainGridFactory());
        }

        /// <summary>
        /// Change the view to the selected Vndb game page
        /// </summary>
        /// <param name="selectedGame"></param>
        public void NavigateVndbHost(UserDataGames selectedGame)
        {
            var vm = this._vndbHostFactory();
            vm.SetSelectedGame(selectedGame);
            this.Delegate?.NavigateTo(vm);
        }

        /// <summary>
        /// Change the view to the selected NoSource game page
        /// </summary>
        /// <param name="selectedGame"></param>
        public void NavigateToNoSource(UserDataGames selectedGame)
        {
            var vm = this._noSourceFactory();
            vm.SetSelectedGame(selectedGame);
            this.Delegate?.NavigateTo(vm);
        }
    }
}
