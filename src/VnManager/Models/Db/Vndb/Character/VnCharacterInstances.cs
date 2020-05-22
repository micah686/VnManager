namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterInstances
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public byte Spoiler { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
    }
}
