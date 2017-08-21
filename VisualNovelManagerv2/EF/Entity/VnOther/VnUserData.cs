using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class VnUserData
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }
    }
}
