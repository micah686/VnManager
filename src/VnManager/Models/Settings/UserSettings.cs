using System;
using VndbSharp.Models.Common;

namespace VnManager.Models.Settings
{
    [Serializable]
    public class UserSettings
    {
        public SexualRating MaxSexualRating { get; set; }
        public ViolenceRating MaxViolenceRating { get; set; }
        public UserSettingsVndb SettingsVndb { get; set; }
        /// <summary>
        /// Is the user required to input a password to unlock the database
        /// </summary>
        public bool RequirePasswordEntry { get; set; } = false;
        /// <summary>
        /// Has the user been asked to import a database dump
        /// </summary>
        public bool DidAskImportDb { get; set; } = false;
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
