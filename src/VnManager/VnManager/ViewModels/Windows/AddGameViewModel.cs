using System;
using System.Collections.Generic;
using System.Text;
using Stylet;

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        private readonly IWindowManager _windowManager;

        public AddGameViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
    }
}
