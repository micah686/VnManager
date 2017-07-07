﻿using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnCharacter
{
    public class VnCharacterTraits
    {
        [Key]
        public int PkId { get; set; }
        public int? CharacterId { get; set; }
        public int? TraitId { get; set; }
        public string TraitName { get; set; }
        public int? SpoilerLevel { get; set; }
    }
}