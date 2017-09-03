using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoScreens:IEntity
    {        
        public int Id { get; set; }
        public uint? VnId { get; set; }
        public string ImageUrl { get; set; }
        public string ReleaseId { get; set; }
        public string Nsfw { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }
}
