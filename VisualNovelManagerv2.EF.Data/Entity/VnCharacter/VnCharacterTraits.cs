using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Data.Entity.VnCharacter
{
    public class VnCharacterTraits
    {
        [Key]
        public int PkId { get; set; }
        public int? CharacterId { get; set; }
        public int? TraitId { get; set; }
        public string TraitName { get; set; }
        public string SpoilerLevel { get; set; }
    }
}
