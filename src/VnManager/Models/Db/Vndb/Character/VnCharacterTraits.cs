namespace VnManager.Models.Db.Vndb.Character
{
    public class VnCharacterTraits
    {
        public int Id { get; set; }
        public uint CharacterId { get; set; }
        public uint TraitId { get; set; }
        public byte SpoilerLevel { get; set; }
    }
}
