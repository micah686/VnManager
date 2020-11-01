using AdonisUI;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VnManager.MetadataProviders;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.Windows;

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

        public void ClickAsync()
        {
            var parent = _container.Get<AddGameMainViewModel>();
            _windowManager.ShowDialog(parent);
            
        }

        
    }

}
