//Used from https://github.com/Onkelsam/VNDBUpdater
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.Converters.TraitConverter.EnumSpoiler
{
    public enum SpoilerSetting
    {
        [Description("Hide all")]
        Hide = 0,

        [Description("Show minor spoilers")]
        ShowMinor,

        [Description("Show all spoilers")]
        ShowAll,
    };
}
