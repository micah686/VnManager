namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterVoiced
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public int StaffAliasId { get; set; }
        public int VnId { get; set; }
        public string Note { get; set; }
    }
}
