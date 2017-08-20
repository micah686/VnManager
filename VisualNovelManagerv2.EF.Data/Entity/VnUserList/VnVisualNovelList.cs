using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace VisualNovelManagerv2.EF.Data.Entity.VnUserList
{
    public class VnVisualNovelList: IEntity
    {
        [Key]
        public int PkId { get; set; }
        public int UserId { get; set; }
        public int VnId { get; set; }
        public string Status { get; set; }
        public string Added { get; set; }
        public string Notes { get; set; }
    }
}
