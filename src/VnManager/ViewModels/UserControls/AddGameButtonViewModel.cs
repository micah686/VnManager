using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using VnManager.ViewModels.Dialogs;

namespace VnManager.ViewModels.UserControls
{
    public class AddGameButtonViewModel: Screen
    {
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public AddGameButtonViewModel(IContainer container, IWindowManager windowManager) 
        {
            _container = container;
            _windowManager = windowManager;
        }

        public void Click()
        {
            var foo = _container.Get<AddGameMultiViewModel>();
            _windowManager.ShowDialog(foo);
            //foo.GameGridPage = (IContainer)_container.Get<GameListViewModel>();
        }
    }

}
