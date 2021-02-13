// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Models;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

namespace VnManager.Helpers.Vndb
{
    public static class VndbDataHelper
    {
        
        public static BindableCollection<VndbInfoViewModel.VnRelationsBinding> LoadRelations()
        {
            if (VndbContentViewModel.VnId == 0)
            {
                return new BindableCollection<VndbInfoViewModel.VnRelationsBinding>();
            }
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return new BindableCollection<VndbInfoViewModel.VnRelationsBinding>();
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var relationCollection = new BindableCollection<VndbInfoViewModel.VnRelationsBinding>();
                var vnRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.VnId && x.Official.ToUpper(CultureInfo.InvariantCulture) == "YES").ToList();
                foreach (var relation in vnRelations)
                {
                    var entry = new VndbInfoViewModel.VnRelationsBinding { RelTitle = relation.Title, RelRelation = relation.Relation };
                    relationCollection.Add(entry);
                }

                return relationCollection;

            }
        }

        public static Tuple<string, Visibility> LoadLinks()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<VnInfoLinks>(DbVnInfo.VnInfo_Links.ToString()).Query()
                .Where(x => x.VnId == VndbContentViewModel.VnId).FirstOrDefault();
            if (dbUserData != null && !string.IsNullOrEmpty(dbUserData.Wikidata))
            {
                var wikiLink = GetWikipediaLink(dbUserData.Wikidata);
                var wikiLinkTuple = !string.IsNullOrEmpty(wikiLink) ? new Tuple<string, Visibility>(wikiLink, Visibility.Visible) : new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
                return wikiLinkTuple;
            }
            else
            {
                return new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            }
        }

        private static string GetWikipediaLink(string wikiDataId)
        {
            var xmlResult = new WebClient().
                DownloadString(@$"https://www.wikidata.org/w/api.php?action=wbgetentities&format=xml&props=sitelinks&ids={wikiDataId}&sitefilter=enwiki");
            XmlSerializer serializer = new XmlSerializer(typeof(WikiDataApi), new XmlRootAttribute("api"));
            StringReader stringReader = new StringReader(xmlResult);
            var xmlData = (WikiDataApi)serializer.Deserialize(stringReader);
            var wikiTitle = xmlData.WdEntities?.WdEntity?.WdSitelinks?.WdSitelink?.Title;
            return wikiTitle;
        }



        public static BindableCollection<BitmapSource> LoadLanguages(ref VnInfo vnInfoEntry)
        {
            if (vnInfoEntry != null)
            {
                var languageCollection = new BindableCollection<BitmapSource>();
                foreach (var language in GetLanguages(vnInfoEntry.Languages))
                {
                    languageCollection.Add(new BitmapImage(new Uri(language)));
                }

                return languageCollection;
            }
            else
            {
                return new BindableCollection<BitmapSource>();
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
    }
}
