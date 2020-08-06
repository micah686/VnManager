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
        public GameCardViewModel(IContainer container)
        {
            _container = container;
        }
        public BitmapSource CoverImage { get; set; }
        public string LastPlayedString { get; set; }
        public string TotalTimeString { get; set; }
        public string Title { get; set; }
        public bool IsNsfwDisabled { get; set; }
        public bool IsMouseOver { get; set; } = false;

        public Guid UserDataId { get; set; }

        public void MouseClick()
        {
            var vm = _container.Get<VndbContentViewModel>();
            RootViewModel.Instance.ActivateItem(vm);
            vm.SetUserDataId(UserDataId);

        }
    }
}
