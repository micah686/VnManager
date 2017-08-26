using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnCharacter
{
    public class VnCharacterTraits: IEntity
    {        
        public int Id { get; set; }
        public int? CharacterId { get; set; }
        public int? TraitId { get; set; }
        public string TraitName { get; set; }
        public string SpoilerLevel { get; set; }
    }
}
