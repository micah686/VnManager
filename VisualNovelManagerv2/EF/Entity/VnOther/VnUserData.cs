using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnOther
{
    public class VnUserData: IEntity
    {        
        public int Id { get; set; }
        public uint? VnId { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }
    }
}
