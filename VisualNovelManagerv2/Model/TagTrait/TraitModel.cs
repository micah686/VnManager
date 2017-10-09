using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbSharp.Models.Common;

namespace VisualNovelManagerv2.Model.TagTrait
{

    public class TraitModel
    {
        public TraitModel(TraitsLookUp RawData)
        {
            ID = RawData.id;
            Name = RawData.name;
            ParentTraits = new List<TraitModel>();
        }

        //public TraitModel(TraitEntity entity)
        //{
        //    ID = entity.ID;
        //    Name = entity.Name;

        //    ParentTraits = entity.ParentTraits?.Select(x => new TraitModel(x)).ToList();
        //    Spoiler = entity.Spoiler;
        //}

        //public TraitModel(List<int> trait, ITraitService TraitService)
        //{
        //    TraitModel foundTrait = TraitService.Get().FirstOrDefault(x => x.ID == trait[0]);

        //    if (foundTrait != null)
        //    {
        //        ID = foundTrait.ID;
        //        Name = foundTrait.Name;
        //        Spoiler = (SpoilerLevel)Enum.Parse(typeof(SpoilerLevel), trait[1].ToString(), true);
        //        ParentTraits = foundTrait.ParentTraits;
        //    }
        //}

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
