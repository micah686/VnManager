// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using Stylet;
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

        public NavigationController(Func<MainGridViewModel> mainGrid, Func<VndbContentViewModel> vndbHost, Func<NoSourceMainViewModel> noSource)
        {
            this._mainGridFactory = mainGrid ?? throw new ArgumentNullException(nameof(mainGrid));
            this._vndbHostFactory = vndbHost;
            this._noSourceFactory = noSource;
        }

        
        public void NavigateToMainGrid()
        {
            this.Delegate?.NavigateTo(this._mainGridFactory());
        }

        public void NavigateVndbHost(UserDataGames selectedGame)
        {
            var vm = this._vndbHostFactory();
            vm.SetSelectedGame(selectedGame);
            this.Delegate?.NavigateTo(vm);
        }

        public void NavigateToNoSource(UserDataGames selectedGame)
        {
            var vm = this._noSourceFactory();
            vm.SetSelectedGame(selectedGame);
            this.Delegate?.NavigateTo(vm);
        }
    }
}
