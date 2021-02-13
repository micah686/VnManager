// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.Helpers.Vndb
{
    public static class VndbAddGameHelper
    {
        public static async Task SetGameDataEntryAsync(AddGameVndbViewModel addGame)
        {
            if (addGame == null)
            {
                return;
            }
            List<UserDataGames> gamesList = new List<UserDataGames>();
            if (addGame.ExeType == ExeType.Collection)
            {
                foreach (var entry in addGame.ExeCollection.Select(item => AddGameMainViewModel.GetDefaultUserDataEntry))
                {
                    entry.SourceType = AddGameSourceType.Vndb;
                    entry.ExeType = addGame.ExeType;
                    entry.GameId = addGame.VnId;
                    entry.ExePath = addGame.ExePath;
                    entry.IconPath = addGame.IconPath;
                    entry.Arguments = addGame.ExeArguments;
                    gamesList.Add(entry);
                }
            }
            else
            {
                var entry = AddGameMainViewModel.GetDefaultUserDataEntry;
                entry.SourceType = AddGameSourceType.Vndb;
                entry.ExeType = addGame.ExeType;
                entry.GameId = addGame.VnId;
                entry.ExePath = addGame.ExePath;
                entry.IconPath = addGame.IconPath;
                entry.Arguments = addGame.ExeArguments;
                gamesList.Add(entry);
            }

            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                dbUserData.Insert(gamesList);
            }

            GetVndbData getData = new GetVndbData();
            await getData.GetDataAsync(addGame.VnId, false);
        }
    }
}
