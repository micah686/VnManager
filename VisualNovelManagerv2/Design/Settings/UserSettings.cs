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
        public VnSettings VnSettings { get; set; }
    }

    public class VnSettings
    {
        public int Id { get; set; }
        public bool NsfwEnabled { get; set; }
        public int Spoiler { get; set; }
    }
}
