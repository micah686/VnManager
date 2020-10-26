using System;
using LiteDB;
using VndbSharp.Models.Common;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoScreens
    {
        [BsonId]
        public int Index { get; set; }
        public uint? VnId { get; set; }
        public string ImageLink { get; set; }
        public string ReleaseId { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public ImageRating ImageRating { get; set; }
    }
}
