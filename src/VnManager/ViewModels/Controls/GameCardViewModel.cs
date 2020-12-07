using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

//using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.Controls
{
    public class GameCardViewModel: Screen
    {
        private readonly IContainer _container;
        public GameCardViewModel(IContainer container)
        {
            _container = container;

        }
        #region CoverImage
        private BindingImage _coverImage;
        public BindingImage CoverImage
        {
            get => _coverImage;
            set
            {
                if (value.IsNsfw == false || ShouldDisplayNsfwContent)
                {
                    _coverImage = value;
                    SetAndNotify(ref _coverImage, value);
                }
                else
                {
                    value.Image = ImageHelper.BlurImage(value.Image, 10);
                    SetAndNotify(ref _coverImage, value);

                }
            }
        }
        #endregion
        public string LastPlayedString { get; set; }
        public string TotalTimeString { get; set; }
        public string Title { get; set; }
        public bool ShouldDisplayNsfwContent { get; set; }
        public bool IsMouseOver { get; set; } = false;


        public Guid UserDataId { get; set; }

        public void MouseClick()
        {

            var vm = _container.Get<VndbContentViewModel>();
            VndbContentViewModel.SetGameId(UserDataId);
            RootViewModel.Instance.ActivateItem(vm);

        }
    }
}
