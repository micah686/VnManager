namespace VnManager.Models.Db.Vndb.Staff
{
    public class VnStaffAliases
    {
        public int Id { get; set; }
        public int? StaffId { get; set; }
        public int? AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
