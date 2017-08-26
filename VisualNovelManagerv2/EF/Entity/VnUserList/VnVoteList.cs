using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnUserList
{
    public class VnVoteList: IEntity
    {        
        public int Id { get; set; }
        public uint UserId { get; set; }
        public uint VnId { get; set; }
        public uint Vote { get; set; }
        public string Added { get; set; }
    }
}
