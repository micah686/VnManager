namespace VisualNovelManagerv2.Model.Settings
{
    public class UserSettings
    {
        public bool NsfwEnabled { get; set; }
        public byte MaxSpoilerLevel { get; set; }
        public VnSetting VnSetting { get; set; }
    }

    public class VnSetting
    {
        public uint Id { get; set; }
        public bool NsfwEnabled { get; set; }
        public uint Spoiler { get; set; }
    }
}
