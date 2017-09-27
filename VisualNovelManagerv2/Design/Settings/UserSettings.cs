using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.Design.Settings
{
    public class UserSettings
    {
        public bool NsfwEnabled { get; set; }
        public uint MaxSpoilerLevel { get; set; }
        public VnSetting VnSetting { get; set; }
    }

    public class VnSetting
    {
        public uint Id { get; set; }
        public bool NsfwEnabled { get; set; }
        public uint Spoiler { get; set; }
    }
}
