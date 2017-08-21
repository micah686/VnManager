using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnRelease
{
    public class VnReleaseMedia: IEntity
    {        
        public int Id { get; set; }
        public int? ReleaseId { get; set; }
        public string Medium { get; set; }
        public int? Quantity { get; set; }
    }
}
