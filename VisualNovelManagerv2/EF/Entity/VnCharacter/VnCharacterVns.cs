using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnCharacter
{
    public class VnCharacterVns: IEntity
    {        
        public int Id { get; set; }
        public int? CharacterId { get; set; }
        public int? VnId { get; set; }
        public int? ReleaseId { get; set; }
        public string SpoilerLevel { get; set; }
        public string Role { get; set; }
    }
}
