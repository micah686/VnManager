using LiteDB;
using VndbSharp.Models.Common;

namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterVns
    {
        [BsonId]
        public int Index { get; set; }
        public uint CharacterId { get; set; }
        public uint VnId { get; set; }
        public uint ReleaseId { get; set; }
        public SpoilerLevel SpoilerLevel { get; set; }
        public string Role { get; set; }
    }
}
