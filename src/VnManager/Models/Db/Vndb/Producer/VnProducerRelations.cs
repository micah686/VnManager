using LiteDB;

namespace VnManager.Models.Db.Vndb.Producer
{
    public class VnProducerRelations
    {
        [BsonId]
        public int Index { get; set; }
        public int? RelationId { get; set; }
        public int? ProducerId { get; set; }
        public string Relation { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
