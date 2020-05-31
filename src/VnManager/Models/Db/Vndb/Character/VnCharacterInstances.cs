﻿using LiteDB;
using VndbSharp.Models.Common;

namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterInstances
    {
        [BsonId]
        public int Index { get; set; }
        public int CharacterId { get; set; }
        public SpoilerLevel Spoiler { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}