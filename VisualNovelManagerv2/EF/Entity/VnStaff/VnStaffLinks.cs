using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnStaff
{
    public class VnStaffLinks
    {
        [Key]
        public int PkId { get; set; }
        public int? StaffId { get; set; }
        public string Homepage { get; set; }
        public string Wikipedia { get; set; }
        public string Twitter { get; set; }
        public string AniDb { get; set; }
    }
}
