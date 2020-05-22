namespace VnManager.Models.Db.Vndb.Release
{
    public class VnReleaseVn
    {
        public int Id { get; set; }
        public uint ReleaseId { get; set; }
        public uint? VnId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
