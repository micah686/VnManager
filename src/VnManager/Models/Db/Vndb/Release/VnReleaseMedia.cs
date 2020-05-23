using LiteDB;

namespace VnManager.Models.Db.Vndb.Release
{
    public class VnReleaseMedia
    {
        [BsonId]
        public int Index { get; set; }
        public uint ReleaseId { get; set; }
        public string Medium { get; set; }
        public uint? Quantity { get; set; }
    }
}
