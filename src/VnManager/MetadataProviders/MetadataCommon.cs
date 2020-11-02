using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders
{
    internal static class MetadataCommon
    {

        public static void SaveUserData(AddItemDbModel data)
        {
            App.StatusBar.IsWorking = true;
            App.StatusBar.StatusString = App.ResMan.GetString("WritingToDb");
            App.StatusBar.IsDatabaseProcessing = true;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                List<UserDataGames> gamesList = new List<UserDataGames>();
                var entry = new UserDataGames();
                if (data.IsCollectionEnabled)
                {
                    foreach (var item in data.ExeCollection)
                    {
                        entry.ExePath = item.ExePath;
                        entry.IconPath = item.IconPath;
                        entry.Arguments = item.ArgumentsString;
                        entry.SourceType = data.SourceType;
                        entry.Id = Guid.NewGuid();
                        entry.GameId = 0;
                        entry.LastPlayed = DateTime.UtcNow;
                        entry.PlayTime = TimeSpan.Zero;
                        gamesList.Add(entry);
                    }
                }
                else
                {
                    entry.SourceType = data.SourceType;
                    entry.Id = Guid.NewGuid();
                    entry.GameId = data.GameId;
                    entry.LastPlayed = DateTime.UtcNow;
                    entry.PlayTime = TimeSpan.Zero;
                    entry.ExePath = data.ExePath;
                    entry.IconPath = data.IconPath;
                    entry.Arguments = data.ExeArguments;
                    //These last 2 should only have data on a NoSourceGame
                    entry.CoverPath = data.CoverPath;
                    entry.Title = data.Title;
                    
                    gamesList.Add(entry);
                }
                dbUserData.Insert(gamesList);
            }

            App.StatusBar.IsWorking = false;
            App.StatusBar.StatusString = "";
            App.StatusBar.IsDatabaseProcessing = false;
        }

        public static async Task SetGameEntryDataAsync(AddItemDbModel gameEntry)
        {
            var addItemDbModel = gameEntry;
            if(addItemDbModel == null) return;
            SaveUserData(gameEntry);

            if (addItemDbModel.SourceType == AddGameSourceType.Vndb)
            {
                GetVndbData getData = new GetVndbData();
                await getData.GetDataAsync(gameEntry.GameId);
            }

        }

    }
}
