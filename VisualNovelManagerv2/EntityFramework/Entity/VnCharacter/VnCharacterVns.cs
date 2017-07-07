using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnCharacter
{
    public class VnCharacterVns
    {
        [Key]
        public int PkId { get; set; }
        public int? CharacterId { get; set; }
        public int? VnId { get; set; }
        public int? ReleaseId { get; set; }
        public string SpoilerLevel { get; set; }
        public string Role { get; set; }
    }
}
