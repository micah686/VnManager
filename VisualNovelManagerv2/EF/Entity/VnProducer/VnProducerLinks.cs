using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnProducer
{
    public class VnProducerLinks: IEntity
    {        
        public int Id { get; set; }
        public int? ProducerId { get; set; }
        public string Homepage { get; set; }
        public string Wikipedia { get; set; }
    }
}
