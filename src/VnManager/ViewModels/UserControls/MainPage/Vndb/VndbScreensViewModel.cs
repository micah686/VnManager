using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbScreensViewModel:Screen
    {
        private readonly IContainer _container;

        public VndbScreensViewModel(IContainer container)
        {
            _container = container;
        }


        public void ShowInfo()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnInfo();
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
        }
    }
}
