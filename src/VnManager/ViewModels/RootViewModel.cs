using MvvmDialogs;
using Stylet;
using StyletIoC;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Conductor<Screen>
    {

        public StatusBarViewModel StatusBarPage { get; set; }

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;

        public static RootViewModel Instance { get; private set; }

        public RootViewModel(IContainer container, IWindowManager windowManager)
        {
            Instance = this;
            _container = container;
            _windowManager = windowManager;

            StatusBarPage = _container.Get<StatusBarViewModel>();


            var maingrid = _container.Get<MainGridViewModel>();
            ActivateItem(maingrid);
        }

    }
}
