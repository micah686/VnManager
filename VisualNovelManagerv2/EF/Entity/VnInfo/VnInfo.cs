using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfo: IEntity
    {        
        public int Id { get; set; }
        public uint VnId { get; set; }
        public string Title { get; set; }
        public string Original { get; set; }
        public string Released { get; set; }
        public string Languages { get; set; }
        public string OriginalLanguage { get; set; }
        public string Platforms { get; set; }
        public string Aliases { get; set; }
        public string Length { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public string ImageNsfw { get; set; }
        public double? Popularity { get; set; }
        public int? Rating { get; set; }
        public virtual VnInfoAnime VnInfoAnime { get; set; }
        public virtual VnInfoLinks VnInfoLinks { get; set; }
        public virtual VnInfoRelations VnInfoRelations { get; set; }
        public virtual VnInfoScreens VnInfoScreens { get; set; }
        public virtual VnInfoStaff VnInfoStaff { get; set; }
        public virtual ICollection<VnInfoTags> VnInfoTags { get; set; }
        public virtual ICollection<VnCharacter.VnCharacter> VnCharacters { get; set; }
        public virtual ICollection<VnRelease.VnRelease> VnReleases { get; set; }
        public virtual ICollection<VnInfoScreens> VnInfoScreensCollection { get; set; }
    }
}
