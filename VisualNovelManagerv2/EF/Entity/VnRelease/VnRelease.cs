using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnRelease
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
        public string Gtin { get; set; }
        public string Catalog { get; set; }
        public string Platforms { get; set; }
        public string Resolution { get; set; }
        public string Voiced { get; set; }
        public string Animation { get; set; }
        //using a csv for animation
        public virtual VnReleaseMedia VnReleaseMedia { get; set; }
        public virtual VnReleaseProducers VnReleaseProducers { get; set; }
        public virtual VnReleaseVn VnReleaseVn { get; set; }
    }
}
