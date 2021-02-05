// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LiteDB;
using VndbSharp.Models.Common;

namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterInfo
    {
        [BsonId]
        public int Index { get; set; }
        public uint? VnId { get; set; }
        public uint CharacterId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string Age { get; set; }
        public string Birthday { get; set; }
        public string Aliases { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public ImageRating ImageRating { get; set; }
        public int? Bust { get; set; }
        public int? Waist { get; set; }
        public int? Hip { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }

    }
}
