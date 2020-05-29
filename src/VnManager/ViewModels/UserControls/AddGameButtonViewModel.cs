using AdonisUI;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using VnManager.ViewModels.Dialogs;

namespace VnManager.ViewModels.UserControls
{
    public class AddGameButtonViewModel: Conductor<Screen>
    {
        public AddGameButtonViewModel(IContainer container, IWindowManager windowManager) 
        {
        }

        public void Click()
        {
            var inst = MainGridViewModel.Instance;
            inst.Click();
            
        }
    }

}
