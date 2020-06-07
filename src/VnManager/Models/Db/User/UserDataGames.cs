using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.Windows;

namespace VnManager.Models.Db.User
{
    public class UserDataGames
    {
        [BsonId]
        public int Index { get; set; }
        public AddGameMainViewModel.AddGameSourceType SourceType { get; set; }
        public AddGameMainViewModel.ExeTypeEnum ExeType { get; set; }
        /// <summary>
        /// Used for a unique value per entry
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Used for various metadata game Ids, like vndb's Vndb ID
        /// </summary>
        public int GameId { get; set; }
        public DateTime LastPlayed { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string Arguments { get; set; }
        public string[] Categories { get; set; }
    }
}
