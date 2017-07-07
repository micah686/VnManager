﻿using System.ComponentModel.DataAnnotations;

namespace VisualNovelManagerv2.EntityFramework.Entity.VnTagTrait
{
    public class VnTagData
    {
        [Key]
        public int PkId { get; set; }
        public int? TagId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Meta { get; set; }
        public int? Vns { get; set; }
        public string Cat { get; set; }
        public string Aliases { get; set; }
        public string Parents { get; set; }
    }
}