using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnRelease
{
    public class VnRelease
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public int? ReleaseId { get; set; }
        public string Title { get; set; }
        public string Original { get; set; }
        public string Released { get; set; }
        public string ReleaseType { get; set; }
        public string Patch { get; set; }
        public string Freeware { get; set; }
        public string Doujin { get; set; }
        public string Languages { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public int? MinAge { get; set; }
        public int? Gtin { get; set; }
        public string Catalog { get; set; }
        public string Platforms { get; set; }
        public string Resolution { get; set; }
        public int? Voiced { get; set; }
        public string Animation { get; set; }
        //using a csv for animation

    }
}
