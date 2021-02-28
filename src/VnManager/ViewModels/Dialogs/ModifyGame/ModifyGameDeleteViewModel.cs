// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
using Stylet;
using VnManager.Events;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameDeleteViewModel: Screen
    {
        internal UserDataGames SelectedGame;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        public ModifyGameDeleteViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            DisplayName = App.ResMan.GetString("DeleteGame");
            _windowManager = windowManager;
            _events = events;
        }

        
        internal void SetSelectedGame(UserDataGames selectedGame)
        {
            SelectedGame = selectedGame;
        }
        
        public void DeleteGame()
        {
            var result = _windowManager.ShowMessageBox(App.ResMan.GetString("DeleteGameCheck"), App.ResMan.GetString("DeleteGame"),
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                switch (SelectedGame.SourceType)
                {
                    case AddGameSourceType.Vndb:
                        DeleteVndbData();
                        break;
                    case AddGameSourceType.NoSource:
                        DeleteNoSourceData();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Delete the Vndb Data
        /// </summary>
        private void DeleteVndbData()
        {
            try
            {
                DeleteVndbContent();
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }

                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                    dbUserData.DeleteMany(x => x.Id == SelectedGame.Id);
                }
                var parent = (ModifyGameHostViewModel)Parent;
                parent.RequestClose();
                _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Delete VndbData");
                SentrySdk.CaptureException(e);
                throw;
            }
        }

        /// <summary>
        /// Delete the Vndb Data from the Db
        /// </summary>
        internal void DeleteVndbContent()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                var vnid = SelectedGame.GameId.Value;
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbInfo = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString());
                    var dbInfoLinks = db.GetCollection<VnInfoLinks>(DbVnInfo.VnInfo_Links.ToString());
                    var dbInfoRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString());
                    var dbInfoScreens = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString());
                    var dbInfoTags = db.GetCollection<VnInfoTags>(DbVnInfo.VnInfo_Tags.ToString());

                    var dbCharacter = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString());
                    var dbCharacterTraits = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString());

                    var charIds = dbCharacter.Query().Where(x => x.VnId == vnid).Select(x => x.CharacterId).ToList();

                    dbInfo.DeleteMany(x => x.VnId == vnid);
                    dbInfoLinks.DeleteMany(x => x.VnId == vnid);
                    dbInfoRelations.DeleteMany(x => x.VnId == vnid);
                    dbInfoScreens.DeleteMany(x => x.VnId == vnid);
                    dbInfoTags.DeleteMany(x => x.VnId == vnid);


                    var charExclude = new List<uint>();
                    foreach (var characterInfo in dbCharacter.FindAll())
                    {
                        if (charIds.Contains(characterInfo.CharacterId) && characterInfo.VnId != vnid)
                        {
                            charExclude.Add(characterInfo.CharacterId);
                        }
                    }

                    var charDeleteIds = charIds.Except(charExclude).ToList();
                    dbCharacter.DeleteMany(x => charDeleteIds.Contains(x.CharacterId));
                    dbCharacterTraits.DeleteMany(x => charDeleteIds.Contains(x.CharacterId));

                }
                DeleteVndbImages(vnid);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to Delete Vndb Data from Db");
                SentrySdk.CaptureException(e);
                throw;
            }


        }
        /// <summary>
        /// Delete Vndb Images
        /// </summary>
        /// <param name="vnId"></param>
        private static void DeleteVndbImages(int vnId)
        {
            try
            {
                string basePath = $@"{App.AssetDirPath}\sources\vndb\images";

                var characters = $@"{basePath}\characters\{vnId}";
                var screenshots = $@"{basePath}\screenshots\{vnId}";
                var cover = $@"{basePath}\cover\{vnId}.jpg";

                if (Directory.Exists(characters))
                {
                    Directory.Delete(characters, true);
                }

                if (Directory.Exists(screenshots))
                {
                    Directory.Delete(screenshots, true);
                }

                if (File.Exists(cover))
                {
                    File.Delete(cover);
                }
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to Delete Vndb Images");
                SentrySdk.CaptureException(e);
                throw;
            }

        }

        /// <summary>
        /// Delete the NoSource Data
        /// </summary>
        private void DeleteNoSourceData()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                    var currentGame = dbUserData.Query().Where(x => x.Id == SelectedGame.Id).FirstOrDefault();
                    if (currentGame == null)
                    {
                        return;
                    }
                    var coverName = $"{Path.Combine(App.AssetDirPath, @"sources\noSource\images\cover\")}{currentGame.Id}{Path.GetExtension(currentGame.CoverPath)}";
                    if (File.Exists(coverName))
                    {
                        File.Delete(coverName);
                    }

                    var bsonId = (BsonValue) currentGame.Index;
                    dbUserData.Delete(bsonId);
                }
                _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to Delete NoSource Data from Db");
                SentrySdk.CaptureException(e);
                throw;
            }
        }
    }
}
