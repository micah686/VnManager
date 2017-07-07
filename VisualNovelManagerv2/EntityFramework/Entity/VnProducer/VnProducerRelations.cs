using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnProducer
{
    public class VnProducerRelations
    {
        [Key]
        public int PkId { get; set; }
        public int? RelationId { get; set; }
        public int? ProducerId { get; set; }
        public string Relation { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
