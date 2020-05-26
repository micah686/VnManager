using LiteDB;

namespace VnManager.Models.Db.Vndb.TagTrait
{
    public class VnTagData
    {
        [BsonId]
        public int Index { get; set; }
        public uint TagId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMeta { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsApplicable { get; set; }
        public uint Vns { get; set; }
        public string Category { get; set; }
        public string Aliases { get; set; }
        public int?[] Parents { get; set; }

        
    }
}
