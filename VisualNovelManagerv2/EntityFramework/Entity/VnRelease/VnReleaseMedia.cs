using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnRelease
{
    public class VnReleaseMedia
    {
        [Key]
        public int PkId { get; set; }
        public int? ReleaseId { get; set; }
        public string Medium { get; set; }
        public int? Quantity { get; set; }
    }
}
