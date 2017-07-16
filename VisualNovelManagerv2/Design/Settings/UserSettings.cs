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
        public int MaxSpoilerLevel { get; set; }
        public VnSetting VnSetting { get; set; }
    }

    public class VnSetting
    {
        public int Id { get; set; }
        public bool NsfwEnabled { get; set; }
        public int Spoiler { get; set; }
    }
}
