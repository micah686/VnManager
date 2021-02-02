using System;
using System.Collections.Generic;
using System.Text;
using Stylet;
using VnManager.Models.Db.User;
using VnManager.ViewModels;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

namespace VnManager
{
    public interface INavigationController
    {
        void NavigateToMainGrid();
        void NavigateVndbHost(UserDataGames selectedGame);
    }

    public interface INavigationControllerDelegate
    {
        void NavigateTo(IScreen screen);
    }

    public class NavigationController : INavigationController
    {
        private readonly Func<MainGridViewModel> _mainGridFactory;
        private readonly Func<VndbContentViewModel> _vndbHostFactory;
        public INavigationControllerDelegate Delegate { get; set; }

        public NavigationController(Func<SettingsViewModel> settings, Func<MainGridViewModel> mainGrid, Func<VndbContentViewModel> vndbHost)
        {
            this._mainGridFactory = mainGrid ?? throw new ArgumentNullException(nameof(mainGrid));
            this._vndbHostFactory = vndbHost;
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
    }
}
