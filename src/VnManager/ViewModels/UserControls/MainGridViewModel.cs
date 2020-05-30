using MvvmDialogs;
using Stylet;
using StyletIoC;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels.UserControls
{
    public class MainGridViewModel: Conductor<Screen>
    {

        public TopInfoBarViewModel TopInfoBarPage { get; set; }
        public LastPlayedViewModel LastPlayedPage { get; set; }
        public CategoryListViewModel CategoryListPage { get; set; }
        public GameGridViewModel GameGridPage { get; set; }
        public AddGameButtonViewModel AddGamePage { get; set; }

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;

        public static MainGridViewModel Instance { get; private set; }

        public MainGridViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService)
        {
            Instance = this;
            _container = container;
            _windowManager = windowManager;

            TopInfoBarPage = _container.Get<TopInfoBarViewModel>();           
            LastPlayedPage = _container.Get<LastPlayedViewModel>();
            CategoryListPage = _container.Get<CategoryListViewModel>();
            AddGamePage = _container.Get<AddGameButtonViewModel>();

            var gg = _container.Get<GameGridViewModel>();
            ActivateItem(gg);
        }


    }
}
