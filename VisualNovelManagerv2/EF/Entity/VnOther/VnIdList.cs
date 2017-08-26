using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class VnIdList: IEntity
    {
        public int Id { get; set; }
        public uint VnId { get; set; }
        public string Title { get; set; }
    }
}
