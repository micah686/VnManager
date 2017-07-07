using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnInfo
{
    public class VnInfoLinks
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public string Wikipedia { get; set; }
        public string Encubed { get; set; }
        public string Renai { get; set; }
    }
}
