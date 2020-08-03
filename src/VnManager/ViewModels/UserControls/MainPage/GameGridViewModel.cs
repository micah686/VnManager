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
        private readonly IWindowManager _windowManager;
        public GameGridViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            //var rand = new Random();
            //for (int i = 0; i < 30; i++)
            //{
            //    var bol = rand.Next() > (Int32.MaxValue / 2);
            //    var bi = new BitmapImage();
            //    bi.BeginInit();
            //    bi.UriSource = new Uri("https://upload.wikimedia.org/wikipedia/en/c/c2/Tron_Legacy_poster.jpg"); /*new Uri("https://s2.vndb.org/cv/23/23223.jpg");*/
            //    bi.EndInit();
            //    var card = new GameCardViewModel(_container, _windowManager)
            //    {
            //        CoverImage = bi,
            //        Title = $"Title {i}",
            //        LastPlayedString = $"Last Played: {i}",
            //        TotalTimeString = $"Total Time: {i}",
            //        IsNsfwDisabled = bol
            //    };
            //    GameCollection.Add(card);
            //}
            GetVndbGames();
        }


        public void GetVndbGames()
        {
            var cred = CredentialManager.GetCredentials("VnManager.DbEnc");
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query().ToEnumerable();
                var dbVnInfo = db.GetCollection<VnInfo>("VnInfo").Query().ToArray();

                foreach (var entry in dbUserData)
                {
                    if (entry.SourceType == AddGameSourceType.Vndb)
                    {
                        var game = dbVnInfo.FirstOrDefault(x => x.VnId == entry.GameId);
                        if(game== null)continue;
                        var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(game.ImageLink)}";
                        

                        var card = new GameCardViewModel(_container, _windowManager)
                        {
                            CoverImage = ImageHelper.GetCoverImage(coverPath),
                            Title = game.Title,
                            LastPlayedString = $"Last Played: {TimeDateChanger.GetHumanDate(entry.LastPlayed)}",
                            TotalTimeString = $"Play Time: {TimeDateChanger.GetHumanTime(entry.PlayTime)}",
                            UserDataId = entry.Id
                        };
                        GameCollection.Add(card);
                    }
                }

            }
        }
    }
}
