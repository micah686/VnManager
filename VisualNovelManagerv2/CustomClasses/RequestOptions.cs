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
        public int? page { get; set; }
        [JsonProperty("results")]
        public int? count { get; set; }
        public string sort { get; set; }
        public bool? reverse { get; set; }
    }
}
