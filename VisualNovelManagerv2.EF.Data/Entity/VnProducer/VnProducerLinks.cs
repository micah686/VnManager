using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Data.Entity.VnProducer
{
    public class VnProducerLinks
    {
        [Key]
        public int PkId { get; set; }
        public int? ProducerId { get; set; }
        public string Homepage { get; set; }
        public string Wikipedia { get; set; }
    }
}
