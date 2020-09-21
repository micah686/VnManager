using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
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



        #region Binding Properties
        public BitmapSource BackgroundImage { get; set; }
        public BitmapSource CoverImage { get; set; }
        public string Title { get; set; }
        public string MainTitle { get; set; }
        public string JpnTitle { get; set; }
        public string Aliases { get; set; }
        public string ReleasedDate { get; set; }
        public string VnLength { get; set; }
        public string Popularity { get; set; }
        public string Rating { get; set; }
        public BindableCollection<BitmapSource> LanguageCollection { get; set; } = new BindableCollection<BitmapSource>();

        public List<Inline> DescriptionInLine { get; set; }

        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }

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
            LoadUserData();
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
                Title = vnInfoEntry.Title;
                MainTitle = $"Title: {vnInfoEntry.Title}";
                JpnTitle = $"Original Title: {vnInfoEntry.Original}";
                Aliases = $"Aliases: {vnInfoEntry.Aliases}";
                ReleasedDate = $"Released: {TimeDateChanger.GetHumanDate(DateTime.Parse(vnInfoEntry.Released))}";
                VnLength = $"Length: {vnInfoEntry.Length}";
                Popularity = vnInfoEntry.Popularity.ToString();//make a UI use this double?
                Rating = vnInfoEntry.Rating.ToString(CultureInfo.InvariantCulture);
                LoadLanguages(ref vnInfoEntry);
                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(vnInfoEntry.ImageLink.AbsoluteUri)}";
                CoverImage = ImageHelper.CreateBitmapFromPath(coverPath);

                DescriptionInLine = BBCodeHelper.Helper(vnInfoEntry.Description);
            }
        }

        private void LoadUserData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query()
                .Where(x => x.Id == UserDataId).FirstOrDefault();
            if (dbUserData != null)
            {
                LastPlayed = TimeDateChanger.GetHumanDate(dbUserData.LastPlayed);
                PlayTime = TimeDateChanger.GetHumanTime(dbUserData.PlayTime);
                _vnId = dbUserData.GameId;
            }
        }

        private void LoadLanguages(ref VnInfo vnInfoEntry)
        {
            foreach (var language in GetLanguages(vnInfoEntry.Languages))
            {
                LanguageCollection.Add(new BitmapImage(new Uri(language)));
            }
        }

        private IEnumerable<string> GetLanguages(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{App.ExecutableDirPath}\Resources\flags\{lang}.png")
                    ? $@"{App.ExecutableDirPath}\Resources\flags\{lang}.png"
                    : $@"{App.ExecutableDirPath}\Resources\flags\_unknown.png")
                .ToList();
        }


        public void ShowInfo()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnInfo();
        }

        public void ShowCharacters()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnCharacters();
        }

        public void ShowScreenshots()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnScreenshots();
        }



    }
}
