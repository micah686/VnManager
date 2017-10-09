//Used from https://github.com/Onkelsam/VNDBUpdater
using VisualNovelManagerv2.Converters.TraitConverter.Models;

namespace VisualNovelManagerv2.Converters.TraitConverter.TraitService
{
    public interface ITraitService : ITagsAndTraits<TraitModel>
    {
        TraitModel GetLastParentTrait(TraitModel trait);
    }
}
