using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnStaff
{
    public class VnStaffAliases
    {
        [Key]
        public int PkId { get; set; }
        public int? StaffId { get; set; }
        public int? AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
