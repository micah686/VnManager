using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnStaff
{
    public class VnStaffAliases: IEntity
    {        
        public int Id { get; set; }
        public int? StaffId { get; set; }
        public int? AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
