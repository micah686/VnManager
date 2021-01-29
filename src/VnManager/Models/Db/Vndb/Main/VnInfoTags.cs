// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LiteDB;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoTags
    {
        [BsonId]
        public int Index { get; set; }
        public uint? VnId { get; set; }
        public uint TagId { get; set; }
        public double Score { get; set; }
        public VndbSharp.Models.Common.SpoilerLevel Spoiler { get; set; }
    }
}
