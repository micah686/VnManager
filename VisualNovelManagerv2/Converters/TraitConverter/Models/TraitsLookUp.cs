//Used from https://github.com/Onkelsam/VNDBUpdater
using System.Collections.Generic;

namespace VisualNovelManagerv2.Converters.TraitConverter.Models
{
    public class TraitsLookUp
    {
        public bool meta { get; set; }
        public int chars { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public List<object> parents { get; set; }
        public int id { get; set; }
        public List<object> aliases { get; set; }
    }
}
