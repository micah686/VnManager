// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
        VnInfo_Links,
        VnInfo_Relations,
        VnInfo_Screens,
        VnInfo_Tags
    }
    /// <summary>
    /// Vndb Character Table Names
    /// </summary>
    internal enum DbVnCharacter
    {
        VnCharacter,
        VnCharacter_Traits
    }
    /// <summary>
    /// Vndb Dump Table Names
    /// </summary>
    internal enum DbVnDump
    {
        VnDump_TagData,
        VnDump_TraitData
    }
    #endregion

    /// <summary>
    /// User Data Table Names
    /// </summary>
    internal enum DbUserData
    {
        UserData_Games,
        UserData_Categories
    }
}
