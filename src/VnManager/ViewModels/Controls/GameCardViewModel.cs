using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using Stylet;
using StyletIoC;
using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.Controls
{
    public class GameCardViewModel: Screen
    {
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public GameCardViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
        }
        public BitmapSource CoverImage { get; set; }
        public string LastPlayedString { get; set; }
        public string TotalTimeString { get; set; }
        public string Title { get; set; }
        public bool IsNsfwDisabled { get; set; }
        public bool IsMouseOver { get; set; } = false;

        public void MouseClick()
        {
            var vm = _container.Get<VndbContentViewModel>();
            RootViewModel.Instance.ActivateItem(vm);
        }
    }
}
