using LiteDB;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoAnime
    {
        [BsonId]
        public int Index { get; set; }
        public uint VnId { get; set; }
        public int? AniDbId { get; set; }
        public int? AnnId { get; set; }
        public string AniNfoId { get; set; }
        public string TitleEng { get; set; }
        public string TitleJpn { get; set; }
        public string Year { get; set; }
        public string AnimeType { get; set; }
    }
}
