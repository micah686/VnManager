namespace VnManager.Models.Db.Vndb.Release
{
    public class VnReleaseProducers
    {
        public int Id { get; set; }
        public uint ReleaseId { get; set; }
        public uint ProducerId { get; set; }
        public bool Developer { get; set; }
        public bool Publisher { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string ProducerType { get; set; }
    }
}
