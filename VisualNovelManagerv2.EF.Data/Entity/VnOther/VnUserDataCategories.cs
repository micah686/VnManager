using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Data.Entity.VnOther
{
    public class VnUserDataCategories
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public int? Category { get; set; }
    }
}
