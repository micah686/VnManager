using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnCharacter
{
    public class VnCharacterTraits: IEntity
    {        
        public int Id { get; set; }
        public uint CharacterId { get; set; }
        public uint TraitId { get; set; }
        public string SpoilerLevel { get; set; }
    }
}
