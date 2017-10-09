//Used from https://github.com/Onkelsam/VNDBUpdater
using System;
using System.Collections.Generic;
using System.Linq;
using VisualNovelManagerv2.Converters.TraitConverter.EnumSpoiler;
using VisualNovelManagerv2.Converters.TraitConverter.TraitService;

namespace VisualNovelManagerv2.Converters.TraitConverter.Models
{
    public class TraitModel
    {
        public TraitModel(TraitsLookUp rawData)
        {
            ID = rawData.id;
            Name = rawData.name;
            ParentTraits = new List<TraitModel>();
        }

        public TraitModel(int trait, ITraitService traitService)
        {
            TraitModel foundTrait = traitService.Get().FirstOrDefault(x => x.ID == trait);

            if (foundTrait != null)
            {
                ID = foundTrait.ID;
                Name = foundTrait.Name;
                Spoiler = (SpoilerLevel)Enum.Parse(typeof(SpoilerLevel), trait.ToString(), true);
                ParentTraits = foundTrait.ParentTraits;
            }
        }

        public int ID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public List<TraitModel> ParentTraits
        {
            get;
            private set;
        }

        public SpoilerLevel Spoiler
        {
            get;
            private set;
        }
    }
}
