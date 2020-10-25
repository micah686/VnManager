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
using MahApps.Metro.IconPacks;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db;

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

        #region Relation Binding

        public BindableCollection<VnRelationsBinding> VnRelations { get; set; } = new BindableCollection<VnRelationsBinding>();

        #endregion


        protected override void OnViewLoaded()
        {
            _vnId = VndbContentViewModel.Instance.VnId;
            GetGameId();
            LoadImage();
            LoadMainData();
            LoadUserData();
            LoadRelations();
        }


        private void GetGameId()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                .Where(x => x.Id == UserDataId).FirstOrDefault();
            if (dbUserData != null)
            {
                _vnId = dbUserData.GameId;
                VndbContentViewModel.Instance.VnId = dbUserData.GameId;
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
                var vnInfoEntry = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().Where(x => x.VnId == _vnId).FirstOrDefault();
                Title = vnInfoEntry.Title;
                MainTitle = $"Title: {vnInfoEntry.Title}";
                JpnTitle = $"Original Title: {vnInfoEntry.Original}";
                Aliases = $"Aliases: {vnInfoEntry.Aliases}";
                ReleasedDate = $"Released: {TimeDateChanger.GetHumanDate(DateTime.Parse(vnInfoEntry.Released, CultureInfo.InvariantCulture))}";
                VnLength = $"Length: {vnInfoEntry.Length}";
                Popularity = vnInfoEntry.Popularity.ToString();//make a UI use this double?
                Rating = vnInfoEntry.Rating.ToString(CultureInfo.InvariantCulture);
                LoadLanguages(ref vnInfoEntry);
                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(vnInfoEntry.ImageLink.AbsoluteUri)}";
                CoverImage = ImageHelper.CreateBitmapFromPath(coverPath);

                DescriptionInLine = BBCodeHelper.Helper(vnInfoEntry.Description);

            }
        }

        private void LoadRelations()
        {
            if (_vnId == 0) return;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString()).Query()
                    .Where(x => x.VnId == _vnId && x.Official.ToUpper(CultureInfo.InvariantCulture) == "YES").ToList();
                foreach (var relation in vnRelations)
                {
                    var entry = new VnRelationsBinding {RelTitle = relation.Title, RelRelation = relation.Relation};
                    VnRelations.Add(entry);
                }
            }
        }


        private void LoadUserData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
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

        private static IEnumerable<string> GetLanguages(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{App.ExecutableDirPath}\Resources\flags\{lang}.png")
                    ? $@"{App.ExecutableDirPath}\Resources\flags\{lang}.png"
                    : $@"{App.ExecutableDirPath}\Resources\flags\_unknown.png")
                .ToList();
        }


        /// <summary>
        /// Reset Vndb Data completely (clear out all images, re-download them all
        /// Then get updated information from the API, overwriting old info
        /// </summary>
        public static void PrepRepairVndbData()
        {
            RepairImages();
        }

        private static void RepairImages()
        {
            string screenshotPath = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\{VndbContentViewModel.Instance.VnId}";
            foreach (var file in Directory.GetFiles(screenshotPath, null, SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
            //TODO:finish this method
        }

        
        public static void ShowCharacters()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnCharacters();
        }

        public static void ShowScreenshots()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnScreenshots();
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            VndbContentViewModel.Instance.Cleanup();
        }

        public class VnRelationsBinding
        {
            public string RelTitle { get; set; }
            public string RelRelation { get; set; }
        }

    }
}
