using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;

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
        internal void SetUserDataId(Guid guid)
        {
            UserDataId = guid;
        }
        #endregion


        private int _vnId;
        public BitmapSource BackgroundImage { get; set; }

        #region Binding Properties
        public string MainTitle { get; set; }
        public string JpnTitle { get; set; }
        public BitmapSource CoverImage { get; set; }


        #endregion



        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public VndbContentViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            
        }

        protected override void OnViewLoaded()
        {
            GetGameId();
            LoadImage();
            LoadMainData();
        }
        private void GetGameId()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query()
                .Where(x => x.Id == UserDataId).FirstOrDefault();
            if (dbUserData != null)
            {
                _vnId = dbUserData.GameId;
            }
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
            BackgroundImage = bs;
        }

        private void LoadMainData()
        {
            if(_vnId ==0)return;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnInfoEntry = db.GetCollection<VnInfo>("VnInfo").Query().Where(x => x.VnId == _vnId).FirstOrDefault();
                MainTitle = vnInfoEntry.Title;
                JpnTitle = vnInfoEntry.Original;

                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(vnInfoEntry.ImageLink)}";
                CoverImage = ImageHelper.GetCoverImage(coverPath);
            }
        }

    }
}
