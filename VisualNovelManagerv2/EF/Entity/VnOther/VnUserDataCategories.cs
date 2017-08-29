using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class VnUserDataCategories: IEntity
    {        
        public int Id { get; set; }
        public int? VnId { get; set; }
        public string Title { get; set; }
    }
}
