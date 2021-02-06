using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

namespace VnManager.ViewModels.UserControls.MainPage.NoSource
{
    public class NoSourceMainViewModel: Screen
    {
        public BitmapSource CoverImage { get; set; }
        public BitmapSource GameIcon { get; set; }
        public string Title { get; set; }
        public Visibility IsStartButtonVisible { get; set; }
        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }
        private UserDataGames _selectedGame;
        
        protected override void OnViewLoaded()
        {
            LoadMainData();
        }

        internal void SetSelectedGame(UserDataGames game)
        {
            _selectedGame = game;

        }

        private void LoadMainData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var gameEntry = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query().Where(x => x.Id == _selectedGame.Id).FirstOrDefault();
                Title = gameEntry.Title;
                GameIcon = ImageHelper.CreateIcon(!string.IsNullOrEmpty(gameEntry.IconPath) ? gameEntry.IconPath : gameEntry.ExePath);
                LastPlayed = TimeDateChanger.GetHumanDate(gameEntry.LastPlayed);
                PlayTime = TimeDateChanger.GetHumanTime(gameEntry.PlayTime);
                var coverName = $"{Path.Combine(App.AssetDirPath, @"sources\noSource\images\cover\")}{gameEntry.Id}.png";
                CoverImage = File.Exists(coverName) ? ImageHelper.CreateBitmapFromPath(coverName) : ImageHelper.CreateEmptyBitmapImage();
                IsStartButtonVisible = Visibility.Visible;
            }
        }
    }
}
