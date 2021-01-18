using LiteDB;

namespace VnManager.Models.Db.Vndb.Producer
{
    public class VnProducer
    {
        [BsonId]
        public int Index { get; set; }
        public int? ProducerId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string ProducerType { get; set; }
        public string Language { get; set; }
        public string Aliases { get; set; }
        public string Description { get; set; }
    }
}
