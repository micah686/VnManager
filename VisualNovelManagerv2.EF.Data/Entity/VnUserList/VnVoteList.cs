using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.EF.Data.Entity.VnUserList
{
    public class VnVoteList: IEntity
    {
        [Key]
        public int PkId { get; set; }
        public int UserId { get; set; }
        public int VnId { get; set; }
        public int Vote { get; set; }
        //should this be a string, or a Time variable?
        public string Added { get; set; }
    }
}
