using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnStaff
{
    public class VnStaff
    {
        [Key]
        public int PkId { get; set; }
        public int? StaffId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Gender { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public int? MainAlias { get; set; }
    }
}
