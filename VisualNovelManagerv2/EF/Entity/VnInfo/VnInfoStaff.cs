using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoStaff
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public int? StaffId { get; set; }
        public int? AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Role { get; set; }
        public string Note { get; set; }
    }
}
