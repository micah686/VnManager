// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Extensions;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.Models;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbInfoViewModel : Screen
    {
        #region Binding Properties
        public BitmapSource CoverImage { get; set; }
        public BitmapSource GameIcon { get; set; }
        public string Title { get; set; }
        public string MainTitle { get; set; }
        public string Aliases { get; set; }
        public string ReleasedDate { get; set; }
        public string VnLength { get; set; }
        public string Popularity { get; set; }
        public string Rating { get; set; }
        public BindableCollection<BitmapSource> LanguageCollection { get; } = new BindableCollection<BitmapSource>();
        public BindableCollection<TagTraitBinding> TagCollection { get; } = new BindableCollection<TagTraitBinding>();
        public Visibility SummaryHeaderVisibility { get; set; }
        public Visibility TagHeaderVisibility { get; set; }
        public Visibility RelationHeaderVisibility { get; set; }
        public Visibility RelationsDataVisibility { get; set; }
        public BindableCollection<VnRelationsBinding> VnRelations { get; } = new BindableCollection<VnRelationsBinding>();
        
        public Inline[] DescriptionInLine { get; private set; }

        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }
        public Visibility IsStartButtonVisible { get; set; }

        public Tuple<string, Visibility> VndbLink { get; set; }
        public Tuple<string, Visibility> WikiLink { get; set; }
        

        #endregion



        protected override void OnViewLoaded()
        {
            VndbLink = new Tuple<string, Visibility>(string.Empty, Visibility.Visible);
            WikiLink = new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            LoadMainData();
            LoadUserData();
            LoadRelations();
            LoadLinks();
            
            TagCollection.Clear();
            TagCollection.AddRange(VndbTagTraitHelper.GetTags(VndbContentViewModel.VnId));
            SummaryHeaderVisibility = DescriptionInLine.Length < 1 ? Visibility.Collapsed : Visibility.Visible;
            TagHeaderVisibility = TagCollection.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            RelationHeaderVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            RelationsDataVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            IsStartButtonVisible = Visibility.Visible;
        }

        private void LoadMainData()
        {
            if (VndbContentViewModel.VnId == 0)
            {
                return;
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnInfoEntry = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().Where(x => x.VnId == VndbContentViewModel.VnId).FirstOrDefault();
                Title = vnInfoEntry.Title;
                MainTitle = vnInfoEntry.Title;
                Aliases = vnInfoEntry.Aliases;
                ReleasedDate = TimeDateChanger.GetHumanDate(DateTime.Parse(vnInfoEntry.Released, CultureInfo.InvariantCulture));
                VnLength = vnInfoEntry.Length;
                Popularity = $"{vnInfoEntry.Popularity:F}";
                Rating = $"{vnInfoEntry.Rating:F}";
                LoadLanguages(ref vnInfoEntry);
                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{vnInfoEntry.VnId}.jpg";
                CoverImage = ImageHelper.CreateBitmapFromPath(coverPath);

                DescriptionInLine = BBCodeHelper.Helper(vnInfoEntry.Description);

            }
        }

        private void LoadUserData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                .Where(x => x.Id == VndbContentViewModel.SelectedGame.Id).FirstOrDefault();
            if (dbUserData != null)
            {
                LastPlayed = TimeDateChanger.GetHumanDate(dbUserData.LastPlayed);
                PlayTime = TimeDateChanger.GetHumanTime(dbUserData.PlayTime);

                GameIcon = ImageHelper.CreateIcon(!string.IsNullOrEmpty(dbUserData.IconPath) ? dbUserData.IconPath : dbUserData.ExePath);
            }
        }
        
        private void LoadRelations()
        {
            if (VndbContentViewModel.VnId == 0)
            {
                return;
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var vnRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.VnId && x.Official.ToUpper(CultureInfo.InvariantCulture) == "YES").ToList();
                foreach (var relation in vnRelations)
                {
                    var entry = new VnRelationsBinding { RelTitle = relation.Title, RelRelation = relation.Relation };
                    VnRelations.Add(entry);
                }

                RelationsDataVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;

            }
        }

        private void LoadLinks()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<VnInfoLinks>(DbVnInfo.VnInfo_Links.ToString()).Query()
                .Where(x => x.VnId == VndbContentViewModel.VnId).FirstOrDefault();
            if (dbUserData != null && !string.IsNullOrEmpty(dbUserData.Wikidata))
            {
                var wikiLink = GetWikipediaLink(dbUserData.Wikidata);
                WikiLink = !string.IsNullOrEmpty(wikiLink) ? new Tuple<string, Visibility>(wikiLink, Visibility.Visible) : new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            }
        }

        public void VndbLinkClick()
        {
            var link = $"https://vndb.org/v{VndbContentViewModel.VnId}";
            var ps = new ProcessStartInfo(link)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        private string GetWikipediaLink(string wikiDataId)
        {
            var xmlResult = new WebClient().DownloadString(@$"https://www.wikidata.org/w/api.php?action=wbgetentities&format=xml&props=sitelinks&ids={wikiDataId}&sitefilter=enwiki");
            XmlSerializer serializer = new XmlSerializer(typeof(WikiDataApi), new XmlRootAttribute("api"));
            StringReader stringReader = new StringReader(xmlResult);
            var xmlData = (WikiDataApi)serializer.Deserialize(stringReader);
            var wikiTitle = xmlData.WdEntities?.WdEntity?.WdSitelinks?.WdSitelink?.Title;
            return wikiTitle;
        }
        
        public void WikiLinkClick()
        {
            if (WikiLink.Item2 == Visibility.Visible)
            {

                var link = $@"https://wikipedia.org/wiki/{WikiLink.Item1}";
                var ps = new ProcessStartInfo(link)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
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
                    : $@"{App.ExecutableDirPath}\Resources\flags\Unknown.png")
                .ToList();
        }


        /// <summary>
        /// Referenced by the Play button on the GUI
        /// <see cref="StartVn"/>
        /// </summary>
        public void StartVn()
        {
            var parent = (VndbContentViewModel) Parent;
            if (parent.IsGameRunning)
            {
                return;
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }

            UserDataGames vnEntry;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                vnEntry = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query().Where(x => x.Id == VndbContentViewModel.SelectedGame.Id).FirstOrDefault();
                
            }

            if (vnEntry?.ExePath != null && Directory.Exists(Path.GetDirectoryName(vnEntry.ExePath)))
            {
                var filepath = $"{vnEntry.ExePath} {vnEntry.Arguments}";
                Directory.SetCurrentDirectory(Path.GetDirectoryName(vnEntry.ExePath));

                var process = new Process {StartInfo = {FileName = filepath}, EnableRaisingEvents = true};
                parent.ProcessList.Add(process);
                process.Exited += VnOrChildProcessExited;
                process.Start();
                parent.IsGameRunning = true;
                parent.GameStopwatch.Start();
                IsStartButtonVisible = Visibility.Collapsed;
                parent.ProcessList.AddRange(process.GetChildProcesses());

            }

        }

        /// <summary>
        /// Referenced by the Stop button on the GUI
        /// <see cref="StopVn"/>
        /// </summary>
        public void StopVn()
        {
            //TODO:Add stop methods here
        }

        private void VnOrChildProcessExited(object sender, EventArgs e)
        {
            var process = (Process) sender;
            if (process == null)
            {
                return;
            }
            var children = process.GetChildProcesses().ToArray();//Get children of exiting process, then attach Exit event handler to each of the children
            if (children.Length >0)
            {
                foreach (var childProcess in children)
                {
                    childProcess.EnableRaisingEvents = true;
                    childProcess.Exited += VnOrChildProcessExited;
                }
                return;
            }
            else
            {
                var parent = (VndbContentViewModel)Parent;
                parent.GameStopwatch.Stop();
                

                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }

                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                    var gameEntry = dbUserData.Query().Where(x => x.Id == VndbContentViewModel.SelectedGame.Id).FirstOrDefault();
                    gameEntry.LastPlayed = DateTime.UtcNow;
                    gameEntry.PlayTime = parent.GameStopwatch.Elapsed;
                    dbUserData.Update(gameEntry);
                }
                parent.GameStopwatch.Reset();
                parent.IsGameRunning = false;
                IsStartButtonVisible = Visibility.Visible;

            }

        }
        
        
        
        
        public class VnRelationsBinding
        {
            public string RelTitle { get; set; }
            public string RelRelation { get; set; }
        }

    }
}
