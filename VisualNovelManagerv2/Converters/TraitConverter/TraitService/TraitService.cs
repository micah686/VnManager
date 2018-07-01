//Used from https://github.com/Onkelsam/VNDBUpdater
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VisualNovelManagerv2.Converters.TraitConverter.Models;

namespace VisualNovelManagerv2.Converters.TraitConverter.TraitService
{
    public class TraitService : ITraitService
    {
        private readonly List<TraitModel> _traits;

        private readonly string _traitsDumpFileName = $@"{Globals.DirectoryPath}\Data\dumps\traits.json";


        public TraitService()
        {
            _traits = new List<TraitModel>();

            GetTraits();
        }

        public IList<TraitModel> Get()
        {
            return _traits;
        }

        public TraitModel GetLastParentTrait(TraitModel trait)
        {
            if (trait.ParentTraits == null)
            {
                return trait;
            }
            if (trait.ParentTraits.Any())
            {
                return GetLastParentTrait(trait.ParentTraits.Last());
            }
            else
            {
                return trait;
            }
        }

        private void GetTraits()
        {
            if (File.Exists(_traitsDumpFileName))
            {
                //get the raw dump of traits
                var rawTraits = JsonConvert.DeserializeObject<List<TraitsLookUp>>(File.ReadAllText(_traitsDumpFileName));

                //adds to a list of traits, with name, spoiler, and parent trait
                foreach (var trait in rawTraits)
                {
                    _traits.Add(new TraitModel(trait));
                }

                foreach (var trait in _traits)
                {
                    //magic linq that adds the names into the parent traits
                    foreach (var parent in rawTraits.Where(x => x.id == trait.ID).Select(x => x.parents).First().ToList())
                    {
                        trait.ParentTraits.AddRange(_traits.Where(x => x.ID.ToString() == parent.ToString()));
                    }
                }
            }
        }
    }
}
