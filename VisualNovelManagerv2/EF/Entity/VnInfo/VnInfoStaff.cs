using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoStaff: IEntity
    {        
        public int Id { get; set; }
        public uint? VnId { get; set; }
        public uint StaffId { get; set; }
        public uint AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Role { get; set; }
        public string Note { get; set; }
    }
}
