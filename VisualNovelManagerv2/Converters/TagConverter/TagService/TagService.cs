using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VndbSharp.Models.Common;

namespace VisualNovelManagerv2.Converters.TagConverter.TagService
{
    public class TagService : ITagService
    {
        private List<TagModel> _Tags;

        private readonly string _TagsDumpFileName = $@"{Globals.DirectoryPath}\Data\dumps\tags.json";


        public TagService()
        {
            _Tags = new List<TagModel>();

            GetTags();
        }

        public IList<TagModel> Get()
        {
            return _Tags;
        }

        public async Task RefreshAsync()
        {

            _Tags = new List<TagModel>();

            GetTags();
        }

        public bool Show(SpoilerSetting UserSetting, SpoilerLevel TSpoiler)
        {
            return (int)UserSetting >= (int)TSpoiler;
        }

        private void GetTags()
        {
            var rawTags = new List<TagsLookUp>();

            if (File.Exists(_TagsDumpFileName))
            {

                rawTags = JsonConvert.DeserializeObject<List<TagsLookUp>>(File.ReadAllText(_TagsDumpFileName));

                foreach (var tag in rawTags)
                {
                    _Tags.Add(new TagModel(tag));
                }
            }
        }
    }        
}
