using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VisualNovelManagerv2.Model.TagTrait
{
    public class TraitService
    {
        private List<TraitModel> _Traits;
        public TraitService()
        {
            _Traits = new List<TraitModel>();

            GetTraits();
        }

        private void GetTraits()
        {
            var rawTraits = new List<TraitsLookUp>();

            if (File.Exists($@"{Globals.DirectoryPath}\Data\dumps\traits.json"))
            {
                //get the raw dump of traits
                rawTraits = JsonConvert.DeserializeObject<List<TraitsLookUp>>(File.ReadAllText($@"{Globals.DirectoryPath}\Data\dumps\traits.json"));

                //adds to a list of traits, with name, spoiler, and parent trait
                foreach (var trait in rawTraits)
                {
                    _Traits.Add(new TraitModel(trait));
                }

                foreach (var trait in _Traits)
                {
                    //magic linq that adds the names into the parent traits
                    foreach (var parent in rawTraits.Where(x => x.id == trait.ID).Select(x => x.parents).First().ToList())
                    {
                        trait.ParentTraits.AddRange(_Traits.Where(x => x.ID.ToString() == parent.ToString()));
                    }
                }
            }
            Thread.Sleep(0);
        }
    }
}
