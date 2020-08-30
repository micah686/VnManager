using System;
using System.Collections.Generic;
using LiteDB;
using VndbSharp.Models.Common;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Release;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfo
    {
        [BsonId]
        public int Index { get; set; }
        public uint VnId { get; set; }
        public string Title { get; set; }
        public string Original { get; set; }
        public string Released { get; set; }
        public string Languages { get; set; }
        public string OriginalLanguages { get; set; }
        public string Platforms { get; set; }
        public string Aliases { get; set; }
        public string Length { get; set; }
        public string Description { get; set; }
        public Uri ImageLink { get; set; }
        public ImageRating ImageRating { get; set; }
        public double? Popularity { get; set; }
        public double Rating { get; set; }
        public int VoteCount { get; set; }
        public virtual VnInfoAnime VnInfoAnime { get; set; }
        public virtual VnInfoLinks VnInfoLinks { get; set; }
        public virtual VnInfoRelations VnInfoRelations { get; set; }
        public virtual VnInfoScreens VnInfoScreens { get; set; }
        public virtual VnInfoStaff VnInfoStaff { get; set; }
        public virtual ICollection<VnInfoTags> VnInfoTags { get; set; }
        public virtual ICollection<VnCharacterInfo> VnCharacters { get; set; }
        public virtual ICollection<VnRelease> VnReleases { get; set; }
        public virtual ICollection<VnInfoScreens> VnInfoScreensCollection { get; set; }
    }
}
