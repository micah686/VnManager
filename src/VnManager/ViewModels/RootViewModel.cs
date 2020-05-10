using MvvmDialogs;
using Stylet;
using StyletIoC;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Conductor<IScreen>
    {
        public TopInfoBarViewModel TopInfoBarPage { get; set; }
        public StatusBarViewModel StatusBarPage { get; set; }
        public LastPlayedViewModel LastPlayedPage { get; set; }
        public CategoryListViewModel CategoryListPage { get; set; }
        public GameGridViewModel GameGridPage { get; set; }
        public AddGameButtonViewModel AddGamePage { get; set; }


        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;

        public RootViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;

            TopInfoBarPage = _container.Get<TopInfoBarViewModel>();
            StatusBarPage = _container.Get<StatusBarViewModel>();
            LastPlayedPage = _container.Get<LastPlayedViewModel>();
            CategoryListPage = _container.Get<CategoryListViewModel>();
            AddGamePage = _container.Get<AddGameButtonViewModel>();

            GameGridPage = _container.Get<GameGridViewModel>();

            //var multi = _container.Get<AddGameViewModel>();
            //var test1 = _windowManager.ShowDialog(multi).Value;
        }

        public void Click()
        {
            var dbg = new DebugTester();
            dbg.Tester();
            //var vm = new AddGameMultiViewModel(_windowManager);
            //_windowManager.ShowWindow(vm);


            //var multi = _container.Get<AddGameMultiViewModel>();
            //var foo = _windowManager.ShowDialog(multi);

            var multi = _container.Get<AddGameViewModel>();
            var test1 = _windowManager.ShowDialog(multi).Value;
            if(test1 == true)
            {
                //var foo1 = multi.GameCollection;
                //multi.Remove();
            }
            

        }
    }
}
