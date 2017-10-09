using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace VisualNovelManagerv2.Model.TagTrait
{
    /// <summary>
    /// Class for (De)Serializing json for Traits
    /// </summary>
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
