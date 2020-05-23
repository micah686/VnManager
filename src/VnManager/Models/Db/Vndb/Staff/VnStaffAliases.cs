using LiteDB;

namespace VnManager.Models.Db.Vndb.Staff
{
    public class VnStaffAliases
    {
        [BsonId]
        public int Index { get; set; }
        public int? StaffId { get; set; }
        public int? AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
