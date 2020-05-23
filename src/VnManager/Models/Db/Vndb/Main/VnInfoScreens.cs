using LiteDB;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoScreens
    {
        [BsonId]
        public int Index { get; set; }
        public uint? VnId { get; set; }
        public string ImageUrl { get; set; }
        public string ReleaseId { get; set; }
        public bool Nsfw { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }
}
