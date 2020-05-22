namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoStaff
    {
        public int Id { get; set; }
        public uint? VnId { get; set; }
        public uint StaffId { get; set; }
        public uint AliasId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Role { get; set; }
        public string Note { get; set; }
    }
}
