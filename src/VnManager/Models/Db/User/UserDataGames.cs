// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using LiteDB;
using VnManager.Extensions;
using VnManager.Helpers;
using VnManager.ViewModels.Dialogs.AddGameSources;

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
        public ExeType ExeType { get; set; }
        public DateTime LastPlayed { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string Arguments { get; set; }
        public Collection<string> Categories { get; internal set; }

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
