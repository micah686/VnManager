using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnUserList
{
    public class VnVisualNovelList: IEntity
    {        
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VnId { get; set; }
        public string Status { get; set; }
        public string Added { get; set; }
        public string Notes { get; set; }
    }
}
