using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.Controls;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.UserControls.MainPage
{
    public class GameGridViewModel: Screen
    {
        public BindableCollection<GameCardViewModel> GameCollection { get; set; } = new BindableCollection<GameCardViewModel>();

        private readonly IContainer _container;
        public GameGridViewModel(IContainer container)
        {
            _container = container;
            GetVndbGames();
        }


        public void GetVndbGames()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query().ToEnumerable();
                var dbVnInfo = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().ToArray();

                foreach (var entry in dbUserData)
                {
                    if (entry.SourceType == AddGameSourceType.Vndb)
                    {
                        var game = dbVnInfo.FirstOrDefault(x => x.VnId == entry.GameId);
                        if(game== null)continue;
                        var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(game.ImageLink)}";

                        var rating = NsfwHelper.TrueIsNsfw(game.ImageRating);

                        var card = new GameCardViewModel(_container);
                        if (rating == true && File.Exists($"{coverPath}.aes"))
                        {
                            var imgBytes = File.ReadAllBytes($"{coverPath}.aes");
                            var imgStream = Secure.DecStreamToStream(new MemoryStream(imgBytes));
                            var imgNsfw = ImageHelper.CreateBitmapFromStream(imgStream);
                            var bi = new BindingImage { Image = imgNsfw, IsNsfw = NsfwHelper.TrueIsNsfw(game.ImageRating) };

                            card.CoverImage = bi;
                            card.Title = game.Title;
                            card.LastPlayedString = $"Last Played: {TimeDateChanger.GetHumanDate(entry.LastPlayed)}";
                            card.TotalTimeString = $"Play Time: {TimeDateChanger.GetHumanTime(entry.PlayTime)}";
                            card.UserDataId = entry.Id;
                            card.IsNsfwDisabled = NsfwHelper.UserIsNsfw(game.ImageRating);
                        }
                        else
                        {
                            var bi = new BindingImage
                            {
                                Image = ImageHelper.CreateBitmapFromPath(coverPath),
                                IsNsfw = false
                            };
                            card.CoverImage = bi;
                            card.Title = game.Title;
                            card.LastPlayedString = $"Last Played: {TimeDateChanger.GetHumanDate(entry.LastPlayed)}";
                            card.TotalTimeString = $"Play Time: {TimeDateChanger.GetHumanTime(entry.PlayTime)}";
                            card.UserDataId = entry.Id;
                            card.IsNsfwDisabled = NsfwHelper.UserIsNsfw(game.ImageRating);
                        }
                        GameCollection.Add(card);
                    }
                }

            }
        }
    }
}
