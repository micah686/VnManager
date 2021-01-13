using Stylet;
using StyletIoC;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.UserControls
{
    public class AddGameButtonViewModel: Conductor<Screen>
    {

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public AddGameButtonViewModel(IContainer container, IWindowManager windowManager) 
        {
            _container = container;
            _windowManager = windowManager;
        }

        public void ShowAddGameDialog()
        {
            var parent = _container.Get<AddGameMainViewModel>();
            _windowManager.ShowDialog(parent);
            
        }

        
    }

}
