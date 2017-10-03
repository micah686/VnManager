using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnCharacter
{
    public class VnCharacterVns: IEntity
    {        
        public int Id { get; set; }
        public uint CharacterId { get; set; }
        public uint VnId { get; set; }
        public uint ReleaseId { get; set; }
        public string SpoilerLevel { get; set; }
        public string Role { get; set; }
    }
}
