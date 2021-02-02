using System;
using System.Collections.Generic;
using System.Text;
using Stylet;
using VnManager.ViewModels;
using VnManager.ViewModels.UserControls;

namespace VnManager
{
    public interface INavigationController
    {
        void NavigateToSettings();

        void NavigateToRootWindow();
        void NavigateToMainGrid();
    }

    public interface INavigationControllerDelegate
    {
        void NavigateTo(IScreen screen);
    }

    public class NavigationController : INavigationController
    {
        private readonly Func<SettingsViewModel> _settingsFactory;
        private readonly Func<MainGridViewModel> _mainGridFactory;
        private readonly Func<RootViewModel> _rootWindowFactory;
        //private readonly Func<Page2ViewModel> page2ViewModelFactory;

        public INavigationControllerDelegate Delegate { get; set; }

        public NavigationController(Func<SettingsViewModel> settings, Func<RootViewModel> rootWindow, Func<MainGridViewModel> mainGrid)
        {
            this._settingsFactory = settings ?? throw new ArgumentNullException(nameof(settings));
            this._rootWindowFactory = rootWindow ?? throw new ArgumentNullException(nameof(rootWindow));
            this._mainGridFactory = mainGrid ?? throw new ArgumentNullException(nameof(mainGrid));
            //this.page2ViewModelFactory = page2ViewModelFactory ?? throw new ArgumentNullException(nameof(page2ViewModelFactory));
        }

        public void NavigateToSettings()
        {
            this.Delegate?.NavigateTo(this._settingsFactory());
        }

        //public void NavigateToPage2(string initiator)
        //{
        //    var vm = this.page2ViewModelFactory();
        //    vm.Initiator = initiator;
        //    this.Delegate?.NavigateTo(vm);
        //}
        public void NavigateToRootWindow()
        {
            this.Delegate?.NavigateTo(this._rootWindowFactory());
        }

        public void NavigateToMainGrid()
        {
            this.Delegate?.NavigateTo(this._mainGridFactory());
        }
    }
}
