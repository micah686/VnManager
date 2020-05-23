using LiteDB;

namespace VnManager.Models.Db.Vndb.Producer
{
    public class VnProducerLinks
    {
        [BsonId]
        public int Index { get; set; }
        public int? ProducerId { get; set; }
        public string Homepage { get; set; }
        public string Wikipedia { get; set; }
    }
}
