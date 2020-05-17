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
        private readonly IDialogService _dialogService;

        public static MainGridViewModel Instance { get; private set; }

        public MainGridViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService)
        {
            Instance = this;
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;

            TopInfoBarPage = _container.Get<TopInfoBarViewModel>();           
            LastPlayedPage = _container.Get<LastPlayedViewModel>();
            CategoryListPage = _container.Get<CategoryListViewModel>();
            AddGamePage = _container.Get<AddGameButtonViewModel>();

            var gg = _container.Get<GameGridViewModel>();
            ActivateItem(gg);
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
            if (test1 == true)
            {
                //var foo1 = multi.GameCollection;
                //multi.Remove();
            }
        }

        public static void TestGrid()
        {
            var foo = Instance._container.Get<GameListViewModel>();
            Instance.ActivateItem(foo);
        }

        public void Test2()
        {
            var foo = _container.Get<GameListViewModel>();
            ActivateItem(foo);
        }



    }
}
