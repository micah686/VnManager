using System;
using System.Collections.Generic;
using System.Text;
using VndbSharp.Interfaces;

namespace VnManager.Helpers.Vndb
{
    public class RequestOptions : IRequestOptions
    {
        public int? Page { get; set; }
        public int? Count { get; set; }
        public string Sort { get; set; }
        public bool? Reverse { get; set; }
    }
}
