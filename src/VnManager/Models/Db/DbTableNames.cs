// ReSharper disable InconsistentNaming

namespace VnManager.Models.Db
{
    #region Vndb Tables
    /// <summary>
    /// Vndb Info Table Names
    /// </summary>
    internal enum DbVnInfo
    {
        VnInfo,
        VnInfo_Anime,
        VnInfo_Links,
        VnInfo_Relations,
        VnInfo_Screens,
        VnInfo_Staff,
        VnInfo_Tags
    }
    /// <summary>
    /// Vndb Character Table Names
    /// </summary>
    internal enum DbVnCharacter
    {
        VnCharacter,
        VnCharacter_Instances,
        VnCharacter_Traits,
        VnCharacter_Vns,
        VnCharacter_Voiced
    }
    /// <summary>
    /// Vndb Dump Table Names
    /// </summary>
    internal enum DbVnDump
    {
        VnDump_TagData,
        VnDump_TraitData
    }
    /// <summary>
    /// Vndb Producer Table Names
    /// </summary>
    internal enum DbVnProducer
    {
        VnProducer,
        VnProducer_Links,
        VnProducer_Relations
    }
    /// <summary>
    /// Vndb Release Table Names
    /// </summary>
    internal enum DbVnRelease
    {
        VnReleases,
        VnRelease_Media,
        VnRelease_Producers,
        VnRelease_Vns
    }
    /// <summary>
    /// Vndb Staff Table Names
    /// </summary>
    internal enum DbVnStaff
    {
        VnStaff,
        VnStaff_Aliases,
        VnStaff_Vns,
        VnStaff_Voiced
    }

    #endregion

    /// <summary>
    /// User Data Table Names
    /// </summary>
    internal enum DbUserData
    {
        UserData_Games
    }
}
