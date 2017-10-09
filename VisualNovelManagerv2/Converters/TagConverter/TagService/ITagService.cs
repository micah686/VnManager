using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.Converters.TagConverter.TagService
{
    public interface ITagService : ITagsAndTraits<TagModel>
    {
    }
    public interface ITagsAndTraits<T>
    {
        IList<T> Get();
    }
}
