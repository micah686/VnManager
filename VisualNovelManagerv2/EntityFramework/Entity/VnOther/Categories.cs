using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnOther
{
    public class Categories
    {
        [Key]
        public int PkId { get; set; }
        public string Category { get; set; }
    }
}
