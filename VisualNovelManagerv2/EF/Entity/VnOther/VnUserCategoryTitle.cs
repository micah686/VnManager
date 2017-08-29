using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class VnUserCategoryTitle
    {
        public int VnUserCategoryTitleId { get; set; }
        public int? VnId { get; set; }
        public string Title { get; set; }

        public List<CategoryJunction> CategoryJunctions { get; set; }
    }
}
