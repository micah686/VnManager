using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoRelations
    {
        [Key]
        public int PkId { get; set; }
        public int? VnId { get; set; }
        public int? RelationId { get; set; }
        public string Relation { get; set; }
        public string Title { get; set; }
        public string Original { get; set; }
        public string Official { get; set; }
    }
}
