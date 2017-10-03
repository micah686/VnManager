using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisualNovelManagerv2.EF.Entity.VnInfo
{
    public class VnInfoLinks: IEntity
    {
        public int Id { get; set; }
        public uint VnId { get; set; }
        public string Wikipedia { get; set; }
        public string Encubed { get; set; }
        public string Renai { get; set; }
    }
}
