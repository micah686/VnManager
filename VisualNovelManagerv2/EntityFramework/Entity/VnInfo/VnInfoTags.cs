using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnInfo
{
    public class VnInfoTags
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public int? TagId { get; set; }
        public string TagName { get; set; }
        public double Score { get; set; }
        public int? Spoiler { get; set; }
    }
}
