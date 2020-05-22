namespace VnManager.Models.Db.Vndb.Release
{
    public class VnReleaseMedia
    {
        public int Id { get; set; }
        public uint ReleaseId { get; set; }
        public string Medium { get; set; }
        public uint? Quantity { get; set; }
    }
}
