using LiteDB;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoLinks
    {
        [BsonId]
        public int Index { get; set; }
        public uint VnId { get; set; }
        public string Wikidata { get; set; }
        public string Encubed { get; set; }
        public string Renai { get; set; }
    }
}
