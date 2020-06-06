using System;
using System.Collections.Generic;
using System.Text;
using MvvmDialogs;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameNoSourceViewModel: Screen
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }



        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;

        public AddGameNoSourceViewModel(IContainer container, IWindowManager windowManager,
            IDialogService dialogService)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;
        }
    }
}
