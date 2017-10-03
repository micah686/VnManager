using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnRelease
{
    public class VnReleaseProducers: IEntity
    {        
        public int Id { get; set; }
        public uint ReleaseId { get; set; }
        public uint ProducerId { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string ProducerType { get; set; }
    }
}
