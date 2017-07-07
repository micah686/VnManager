using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnRelease
{
    public class VnReleaseVn
    {
        [Key]
        public int PkId { get; set; }
        public int? ReleaseId { get; set; }
        public int? VnId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
