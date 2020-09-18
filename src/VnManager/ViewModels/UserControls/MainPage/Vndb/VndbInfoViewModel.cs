using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbInfoViewModel : Screen
    {
        public Guid UserDataId { get; private set; }
        internal void SetUserDataId(Guid guid)
        {
            UserDataId = guid;
        }

        private int _vnId;
        public BitmapSource BackgroundImage { get; set; }


        #region Binding Properties
        public string MainTitle { get; set; }
        public string JpnTitle { get; set; }
        public BitmapSource CoverImage { get; set; }

        #endregion




        private readonly IContainer _container;
        public VndbInfoViewModel(IContainer container)
        {
            _container = container;
        }

        protected override void OnViewLoaded()
        {
            GetGameId();
            LoadImage();
            LoadMainData();
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
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

        private void LoadImage()
        {
            try
            {
                var filePath = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\4\8369.jpg";
                var uri = new Uri(filePath);
                BitmapSource bs = new BitmapImage(uri);
                BackgroundImage = bs;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                //throw;
            }

        }

        private void LoadMainData()
        {
            if (_vnId == 0) return;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnInfoEntry = db.GetCollection<VnInfo>("VnInfo").Query().Where(x => x.VnId == _vnId).FirstOrDefault();
                MainTitle = vnInfoEntry.Title;
                JpnTitle = vnInfoEntry.Original;

                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(vnInfoEntry.ImageLink.AbsoluteUri)}";
                CoverImage = ImageHelper.CreateBitmapFromPath(coverPath);
            }
        }

        public void ShowScreenshots()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateScreenshots();
        }



    }
}
