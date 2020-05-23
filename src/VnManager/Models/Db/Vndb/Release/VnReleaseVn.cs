using LiteDB;

namespace VnManager.Models.Db.Vndb.Release
{
    public class VnReleaseVn
    {
        [BsonId]
        public int Index { get; set; }
        public uint ReleaseId { get; set; }
        public uint? VnId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
