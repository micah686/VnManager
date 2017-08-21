using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnUserList
{
    public class VnVoteList: IEntity
    {        
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VnId { get; set; }
        public int Vote { get; set; }
        //should this be a string, or a Time variable?
        public string Added { get; set; }
    }
}
