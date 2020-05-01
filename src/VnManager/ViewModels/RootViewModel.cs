using MvvmDialogs;
using Stylet;
using StyletIoC;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Screen
    {
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;

        public RootViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;
        }

        public void Click()
        {
            var dbg = new DebugTester();
            dbg.Tester();
            //var vm = new AddGameMultiViewModel(_windowManager);
            //_windowManager.ShowWindow(vm);


            //var multi = _container.Get<AddGameMultiViewModel>();
            //var foo = _windowManager.ShowDialog(multi);

            var multi = _container.Get<AddGameMultiViewModel>();
            var test1 = _windowManager.ShowDialog(multi).Value;
            if(test1 == true)
            {
                var foo1 = multi.GameCollection;
                multi.Remove();
            }
            

        }
    }
}
