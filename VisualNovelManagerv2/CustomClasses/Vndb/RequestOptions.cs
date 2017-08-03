using VndbSharp.Interfaces;

namespace VisualNovelManagerv2.CustomClasses.Vndb
{
    public class RequestOptions: IRequestOptions
    {
        public int? Page { get; set; }
        public int? Count { get; set; }
        public string Sort { get; set; }
        public bool? Reverse { get; set; }
    }
}
