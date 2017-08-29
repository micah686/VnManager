using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class CategoryJunction
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int VnUserCategoryTitleId { get; set; }
        public VnUserCategoryTitle VnUserCategoryTitle { get; set; }
    }
}
