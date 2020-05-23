using LiteDB;
using VndbSharp.Models.Common;

namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterTraits
    {
        [BsonId]
        public int Index { get; set; }
        public uint CharacterId { get; set; }
        public uint TraitId { get; set; }
        public SpoilerLevel SpoilerLevel { get; set; }
    }
}
