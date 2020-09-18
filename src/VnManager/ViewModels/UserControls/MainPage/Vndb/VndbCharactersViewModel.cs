using System;
using System.Collections.Generic;
using System.Text;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbCharactersViewModel: Screen
    {
        private readonly IContainer _container;

        public VndbCharactersViewModel(IContainer container)
        {
            _container = container;
        }
    }
}
