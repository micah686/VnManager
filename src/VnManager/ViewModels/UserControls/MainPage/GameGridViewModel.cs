// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Events;
using VnManager.Helpers;
using VnManager.Interfaces;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.Controls;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.UserControls.MainPage
{
    public class GameGridViewModel: Screen, IHandle<UpdateEvent>
    {
        public BindableCollection<GameCardViewModel> GameCollection { get; } = new BindableCollection<GameCardViewModel>();

        private readonly IGameCardFactory _gameCard;
        public GameGridViewModel(IEventAggregator events, IGameCardFactory gameCard)
        {
            _gameCard = gameCard;

            SetupEvents(events);

            GetGameData();
        }

        private void SetupEvents(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this, EventChannels.RefreshGameGrid.ToString());
        }

        private void GetGameData()
        {
            List<UserDataGames> dbUserData = new List<UserDataGames>();
            List<VnInfo> dbVnInfo = new List<VnInfo>();
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbAll = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query().ToList();
                GameCollection.Clear();
                foreach (var userData in dbAll)
                {
                    if (userData.Categories != null && userData.Categories.Contains(CategoryListViewModel.SelectedCategory))
                    {
                        dbUserData.Add(userData);
                    }
                }
                dbVnInfo.AddRange(db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().ToList());
            }

            var userDataGamesEnumerable = dbUserData.ToArray();
            var vndbData = userDataGamesEnumerable.Where(x => x.SourceType == AddGameSourceType.Vndb).ToArray();
            var noSourceData = userDataGamesEnumerable.Where(x => x.SourceType == AddGameSourceType.NoSource).ToArray();
            if (noSourceData.Length > 0)
            {
                
                CreateNoSourceCards(noSourceData);
            }

            if (vndbData.Length > 0)
            {
                GetVndbData(vndbData, dbVnInfo.ToArray());
            }
            
        }


        private void GetVndbData(UserDataGames[] userDataArray, VnInfo[] vndbInfo)
        {
            foreach (var entry in userDataArray)
            {
                var game = vndbInfo.FirstOrDefault(x => x.VnId == entry.GameId);
                if(game == null)
                {
                    continue;
                }
                var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{game.VnId}.jpg";

                var rating = NsfwHelper.RawRatingIsNsfw(game.ImageRating);

                var card = _gameCard.CreateGameCard();
                if (rating == true && File.Exists($"{coverPath}.aes"))
                {
                    var imgBytes = File.ReadAllBytes($"{coverPath}.aes");
                    var imgStream = Secure.DecStreamToStream(new MemoryStream(imgBytes));
                    var imgNsfw = ImageHelper.CreateBitmapFromStream(imgStream);
                    var bi = new BindingImage { Image = imgNsfw, IsNsfw = NsfwHelper.RawRatingIsNsfw(game.ImageRating) };

                    card.CoverImage = bi;
                    card.Title = game.Title;
                    card.LastPlayedString = $"{App.ResMan.GetString("LastPlayed")}: {TimeDateChanger.GetHumanDate(entry.LastPlayed)}";
                    card.TotalTimeString = $"{App.ResMan.GetString("PlayTime")}: {TimeDateChanger.GetHumanTime(entry.PlayTime)}";
                    card.UserDataId = entry.Id;
                    card.ShouldDisplayNsfwContent = !NsfwHelper.UserIsNsfw(game.ImageRating);
                }
                else
                {
                    var bi = new BindingImage
                    {
                        Image = ImageHelper.CreateEmptyBitmapImage(),
                        IsNsfw = false
                    };
                    if (File.Exists(coverPath))
                    {
                        bi = new BindingImage
                        {
                            Image = ImageHelper.CreateBitmapFromPath(coverPath),
                            IsNsfw = false
                        };
                    }
                    card.CoverImage = bi;
                    card.Title = game.Title;
                    card.LastPlayedString = $"Last Played: {TimeDateChanger.GetHumanDate(entry.LastPlayed)}";
                    card.TotalTimeString = $"Play Time: {TimeDateChanger.GetHumanTime(entry.PlayTime)}";
                    card.UserDataId = entry.Id;
                    card.ShouldDisplayNsfwContent = !NsfwHelper.UserIsNsfw(game.ImageRating);
                }
                GameCollection.Add(card);
            }
        }

        private void CreateNoSourceCards(UserDataGames[] userDataArray)
        {
            foreach (var entry in userDataArray)
            {
                //TODO:Add in cover and ratings
                var card = _gameCard.CreateGameCard();
                card.Title = entry.Title;
                card.LastPlayedString = $"{App.ResMan.GetString("LastPlayed")}: {TimeDateChanger.GetHumanDate(entry.LastPlayed)}";
                card.TotalTimeString = $"{App.ResMan.GetString("PlayTime")}: {TimeDateChanger.GetHumanTime(entry.PlayTime)}";
                card.CoverImage = new BindingImage {Image = ImageHelper.CreateEmptyBitmapImage()};
                card.UserDataId = entry.Id;


                GameCollection.Add(card);
            }
        }
        
        public void Handle(UpdateEvent message)
        {
            if (message != null && message.ShouldUpdate)
            {
                GetGameData();
            }
            
        }
    }
}
