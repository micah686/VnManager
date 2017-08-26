using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnRelease
{
    public class VnReleaseProducers: IEntity
    {        
        public int Id { get; set; }
        public int? ReleaseId { get; set; }
        public int? ProducerId { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string ProducerType { get; set; }
    }
}
