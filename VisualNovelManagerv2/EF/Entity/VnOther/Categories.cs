using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class Categories: IEntity
    {        
        public int Id { get; set; }
        public string Category { get; set; }
        public virtual ICollection<VnUserDataCategories> VnUserDataCategories { get; set; }
    }
}
