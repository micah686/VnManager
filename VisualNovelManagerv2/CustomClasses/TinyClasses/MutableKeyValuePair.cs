using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.CustomClasses.TinyClasses
{
    public class MutableKeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public MutableKeyValuePair(TKey item1, TValue item2)
        {
            this.Key = item1;
            this.Value = item2;
        }
    }
}
