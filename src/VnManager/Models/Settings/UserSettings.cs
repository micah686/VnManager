using System;
using System.Collections.Generic;
using System.Text;
using VndbSharp.Models.Common;

namespace VnManager.Models.Settings
{
    [Serializable]
    public class UserSettings
    {
        public string ColorTheme { get; set; }
        public bool IsNsfwEnabled { get; set; }
        public bool IsVisibleSavedNsfwContent { get; set; }
        public UserSettingsVndb SettingsVndb { get; set; }
    }

    public class UserSettingsVndb
    {
        public uint CurrentId { get; set; }
        public SpoilerLevel Spoiler { get; set; }

    }
}
