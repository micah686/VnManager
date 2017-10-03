using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnRelease
{
    public class VnReleaseMedia: IEntity
    {        
        public int Id { get; set; }
        public uint ReleaseId { get; set; }
        public string Medium { get; set; }
        public uint? Quantity { get; set; }
    }
}
