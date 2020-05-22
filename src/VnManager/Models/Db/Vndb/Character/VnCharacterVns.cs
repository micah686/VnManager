namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterVns
    {
        public int Id { get; set; }
        public uint CharacterId { get; set; }
        public uint VnId { get; set; }
        public uint ReleaseId { get; set; }
        public byte SpoilerLevel { get; set; }
        public string Role { get; set; }
    }
}
