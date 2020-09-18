using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
