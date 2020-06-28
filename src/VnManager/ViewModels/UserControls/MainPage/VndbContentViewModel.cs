using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.UserControls.MainPage
{
    public class VndbContentViewModel: Screen
    {
        #region UserGuid
        private Guid _userDataId;
        public Guid UserDataId
        {
            get => _userDataId;
            set
            {
                if (_userDataId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                {
                    _userDataId = value;
                }
            }
        }
        #endregion

        private int vnId;
        public BitmapSource CoverSource { get; set; }

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public VndbContentViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            LoadImage();
        }

        internal void SetUserDataId(Guid guid)
        {
            UserDataId = guid;
        }

        public void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
        }


        private void LoadImage()
        {
            var filePath = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\2002\41869.jpg";
            var uri = new Uri(filePath);
            BitmapSource bs = new BitmapImage(uri);
            CoverSource = bs;
        }
    }
}
