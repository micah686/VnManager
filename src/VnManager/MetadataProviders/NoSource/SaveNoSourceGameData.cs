using System;
using System.Collections.Generic;
using System.Text;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Models;
using VnManager.Models.Db.User;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders.NoSource
{
    public class SaveNoSourceGameData
    {
        public void SaveUserData(AddItemDbModel data)
        {
            App.StatusBar.IsWorking = true;
            App.StatusBar.StatusString = App.ResMan.GetString("WritingToDb");
            App.StatusBar.IsDatabaseProcessing = true;
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games");
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
                    entry.GameId = 0;
                    entry.LastPlayed = DateTime.UtcNow;
                    entry.PlayTime = TimeSpan.Zero;
                    entry.ExePath = data.ExePath;
                    entry.IconPath = data.IconPath;
                    entry.Arguments = data.ExeArguments;
                    gamesList.Add(entry);
                }
                dbUserData.Insert(gamesList);
            }

            App.StatusBar.IsWorking = false;
            App.StatusBar.StatusString = "";
            App.StatusBar.IsDatabaseProcessing = false;
        }
    }
}
