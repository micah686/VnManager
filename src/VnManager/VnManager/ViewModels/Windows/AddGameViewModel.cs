using System;
using System.Collections.Generic;
using System.Text;
using Stylet;

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        public int VnId { get; set; }
        public string VnName { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }

        private readonly IWindowManager _windowManager;

        public AddGameViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
    }
}
