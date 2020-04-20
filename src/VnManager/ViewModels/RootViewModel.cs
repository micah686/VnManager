using Stylet;
using System;
using System.Collections.Generic;
using System.Text;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Screen
    {
        private readonly IWindowManager _windowManager;

        public RootViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Click()
        {
            var dbg = new DebugTester();
            dbg.Tester();
            var vm = new AddGameViewModel(_windowManager);
            _windowManager.ShowWindow(vm);
        }
    }
}
