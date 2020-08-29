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
        public SexualRating MaxSexualRating { get; set; }
        public ViolenceRating MaxViolenceRating { get; set; }
        public bool IsVisibleSavedNsfwContent { get; set; }
        public UserSettingsVndb SettingsVndb { get; set; }
        public bool EncryptionEnabled { get; set; } = false;
    }

    public class UserSettingsVndb
    {
        public SpoilerLevel Spoiler { get; set; }

    }

    public enum SexualRating
    {
        Safe=0,
        Suggestive =1,
        Explicit =2
    }

    public enum ViolenceRating
    {
        Tame=0,
        Violent=1,
        Brutal=2
    }
}
