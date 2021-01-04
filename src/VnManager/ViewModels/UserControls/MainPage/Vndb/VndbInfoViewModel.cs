using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using MahApps.Metro.IconPacks;
using Stylet;
using VndbSharp.Models.Common;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.Models.Settings;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbInfoViewModel : Screen
    {
        #region Binding Properties
        public BitmapSource CoverImage { get; set; }
        public string Title { get; set; }
        public string MainTitle { get; set; }
        public string Aliases { get; set; }
        public string ReleasedDate { get; set; }
        public string VnLength { get; set; }
        public string Popularity { get; set; }
        public string Rating { get; set; }
        public BindableCollection<BitmapSource> LanguageCollection { get; set; } = new BindableCollection<BitmapSource>();
        public BindableCollection<TagTraitBinding> TagCollection { get; set; } = new BindableCollection<TagTraitBinding>();
        public Visibility SummaryHeaderVisibility { get; set; }
        public Visibility TagHeaderVisibility { get; set; }
        public Visibility RelationHeaderVisibility { get; set; }

        public List<Inline> DescriptionInLine { get; set; }

        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }

        #endregion

        #region Relation Binding

        public BindableCollection<VnRelationsBinding> VnRelations { get; set; } = new BindableCollection<VnRelationsBinding>();

        #endregion

        protected override void OnViewLoaded()
        {
            LoadMainData();
            LoadUserData();
            LoadRelations();

            
            TagCollection.Clear();
            TagCollection.AddRange(VndbTagTraitHelper.GetTags(VndbContentViewModel.VnId));
            SummaryHeaderVisibility = DescriptionInLine.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            TagHeaderVisibility = TagCollection.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            RelationHeaderVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadMainData()
        {
            if (VndbContentViewModel.VnId == 0) return;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnInfoEntry = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().Where(x => x.VnId == VndbContentViewModel.VnId).FirstOrDefault();
                Title = vnInfoEntry.Title;
                MainTitle = vnInfoEntry.Title;
                Aliases = vnInfoEntry.Aliases;
                ReleasedDate = TimeDateChanger.GetHumanDate(DateTime.Parse(vnInfoEntry.Released, CultureInfo.InvariantCulture));
                VnLength = vnInfoEntry.Length;
                Popularity = vnInfoEntry.Popularity.ToString();//make a UI use this double?
                Rating = vnInfoEntry.Rating.ToString(CultureInfo.InvariantCulture);
                LoadLanguages(ref vnInfoEntry);
                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(vnInfoEntry.ImageLink)}";
                CoverImage = ImageHelper.CreateBitmapFromPath(coverPath);

                DescriptionInLine = BBCodeHelper.Helper(vnInfoEntry.Description);

            }
        }

        private void LoadUserData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                .Where(x => x.Id == VndbContentViewModel.UserDataId).FirstOrDefault();
            if (dbUserData != null)
            {
                LastPlayed = TimeDateChanger.GetHumanDate(dbUserData.LastPlayed);
                PlayTime = TimeDateChanger.GetHumanTime(dbUserData.PlayTime);
            }
        }
        
        private void LoadRelations()
        {
            if (VndbContentViewModel.VnId == 0) return;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.VnId && x.Official.ToUpper(CultureInfo.InvariantCulture) == "YES").ToList();
                foreach (var relation in vnRelations)
                {
                    var entry = new VnRelationsBinding { RelTitle = relation.Title, RelRelation = relation.Relation };
                    VnRelations.Add(entry);
                }
            }
        }

        

        private void LoadLanguages(ref VnInfo vnInfoEntry)
        {
            LanguageCollection.Clear();
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


        public class VnRelationsBinding
        {
            public string RelTitle { get; set; }
            public string RelRelation { get; set; }
        }

    }
}
