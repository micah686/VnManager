using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EF.Entity.VnCharacter
{
    public class VnCharacter: IEntity
    {
        public int Id { get; set; }
        public uint? VnId { get; set; }
        public uint CharacterId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string Birthday { get; set; }
        public string Aliases { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public int? Bust { get; set; }
        public int? Waist { get; set; }
        public int? Hip { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public virtual ICollection<VnCharacterTraits> VnCharacterTraits { get; set; }
        public virtual VnCharacterVns VnCharacterVns { get; set; }
        
    }
}
