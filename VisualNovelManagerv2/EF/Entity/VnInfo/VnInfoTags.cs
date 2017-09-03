using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoTags: IEntity
    {        
        public int Id { get; set; }
        public uint? VnId { get; set; }
        public int? TagId { get; set; }
        public string TagName { get; set; }
        public double Score { get; set; }
        public string Spoiler { get; set; }
    }
}
