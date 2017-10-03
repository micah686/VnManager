using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoAnime: IEntity
    {        
        public int Id { get; set; }
        public uint VnId { get; set; }
        public int? AniDbId { get; set; }
        public int? AnnId { get; set; }
        public string AniNfoId { get; set; }
        public string TitleEng { get; set; }
        public string TitleJpn { get; set; }
        public string Year { get; set; }
        public string AnimeType { get; set; }
    }
}
