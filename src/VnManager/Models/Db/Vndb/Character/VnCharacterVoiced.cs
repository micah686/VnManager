using LiteDB;

namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterVoiced
    {
        [BsonId]
        public int Index { get; set; }
        public int CharacterId { get; set; }
        public int StaffId { get; set; }
        public int StaffAliasId { get; set; }
        public int VnId { get; set; }
        public string Note { get; set; }
    }
}
