using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualNovelManagerv2.Converters.TagConverter.TagService;
using VndbSharp.Models.Common;
using SpoilerLevel = VisualNovelManagerv2.Converters.TagConverter.TagService.SpoilerLevel;

namespace VisualNovelManagerv2.Converters.TagConverter
{
    public class TagModel
    {
        public TagModel(TagsLookUp RawData)
        {
            ID = RawData.id;
            Name = RawData.name;
            Description = RawData.description;
            Category = (TagCategory)Enum.Parse(typeof(TagCategory), RawData.cat);
        }

        public TagModel(TagEntity entity)
        {
            ID = entity.ID;
            Name = entity.Name;
            Description = entity.Description;
            Score = entity.Score;
            Category = entity.Category;
            Spoiler = entity.Spoiler;
        }

        public TagModel(List<double> tag, ITagService TagService)
        {
            TagModel foundTag = TagService.Get().FirstOrDefault(x => x.ID == tag[0]);

            if (foundTag != null)
            {
                ID = foundTag.ID;
                Category = foundTag.Category;
                Description = foundTag.Description;
                Name = foundTag.Name;
                Score = tag[1];
                Spoiler = (SpoilerLevel)Enum.Parse(typeof(SpoilerLevel), tag[2].ToString(), true);
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

        public string Description
        {
            get;
            private set;
        }

        public double Score
        {
            get;
            private set;
        }

        public TagCategory Category
        {
            get;
            private set;
        }

        public SpoilerLevel Spoiler
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }

        public enum TagCategory : byte
        {
            All = 0,
            cont,
            ero,
            tech
        };
    }
    public class TagEntity
    {
        public TagEntity()
        {
        }

        public TagEntity(TagModel model)
        {
            ID = model.ID;
            Name = model.Name;
            Description = model.Description;
            Category = model.Category;
            Spoiler = model.Spoiler;
            Score = model.Score;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TagModel.TagCategory Category { get; set; }
        public SpoilerLevel Spoiler { get; set; }
        public double Score { get; set; }
    }
}
