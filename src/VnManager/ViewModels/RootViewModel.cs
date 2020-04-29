using Stylet;
using StyletIoC;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Screen
    {
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;

        public RootViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
        }

        public void Click()
        {
            var dbg = new DebugTester();
            dbg.Tester();
            //var vm = new AddGameMultiViewModel(_windowManager);
            //_windowManager.ShowWindow(vm);


            var multi = _container.Get<AddGameMultiViewModel>();
            var foo = _windowManager.ShowDialog(multi);

        }
    }
}
