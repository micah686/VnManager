//Used from https://github.com/Onkelsam/VNDBUpdater
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.Converters.TraitConverter.TraitService
{
    public interface ITagsAndTraits<T>
    {
        IList<T> Get();
    }
}
