using LiteDB;

namespace VnManager.Models.Db.Vndb.Staff
{
    public class VnStaffVns
    {
        [BsonId]
        public int Index { get; set; }
        public int VnId { get; set; }
        public int? StaffId { get; set; }
        public int AliasId { get; set; }
        public string Role { get; set; }
        public string Note { get; set; }
    }
}
