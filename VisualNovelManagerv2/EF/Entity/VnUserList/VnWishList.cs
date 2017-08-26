using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnUserList
{
    public class VnWishList: IEntity
    {        
        public int Id { get; set; }
        public uint UserId { get; set; }
        public uint VnId { get; set; }
        public string Priority { get; set; }
        public string Added { get; set; }
    }
}
