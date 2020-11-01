using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using VnManager.Extensions;
using VnManager.Helpers;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.Windows;

namespace VnManager.Models.Db.User
{
    public class UserDataGames: ValidationBase
    {
        [BsonId]
        public int Index { get; set; }
        /// <summary>
        /// Used for a unique value per entry
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Used for various metadata game Ids, like vndb's Vndb ID
        /// </summary>
        public Stringable<int> GameId { get; set; }
        public string GameName { get; set; }
        public AddGameSourceType SourceType { get; set; }
        public ExeTypeEnum ExeType { get; set; }
        public DateTime LastPlayed { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string Arguments { get; set; }
        public string[] Categories { get; set; }

        /// <summary>
        /// CoverPath should ONLY be used for NoSource games
        /// </summary>
        public string CoverPath { get; set; }
        /// <summary>
        /// Title should ONLY be used for NoSource games
        /// </summary>
        public string Title { get; set; }
    }
}
