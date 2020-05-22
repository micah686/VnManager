namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoTags
    {
        public int Index { get; set; }
        public uint? VnId { get; set; }
        public uint TagId { get; set; }
        public double Score { get; set; }
        public VndbSharp.Models.Common.SpoilerLevel Spoiler { get; set; }
    }
}
