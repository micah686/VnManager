using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VndbSharp.Interfaces;

namespace VisualNovelManagerv2.CustomClasses
{
    public class RequestOptions: IRequestOptions
    {
        public int? Page { get; set; }
        public int? Count { get; set; }
        public string Sort { get; set; }
        public bool? Reverse { get; set; }
    }
}
