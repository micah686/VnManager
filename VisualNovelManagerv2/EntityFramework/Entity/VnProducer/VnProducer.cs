using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnProducer
{
    public class VnProducer
    {
        [Key]
        public int PkId { get; set; }
        public int? ProducerId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string ProducerType { get; set; }
        public string Language { get; set; }
        public string Aliases { get; set; }
        public string Description { get; set; }
    }
}
