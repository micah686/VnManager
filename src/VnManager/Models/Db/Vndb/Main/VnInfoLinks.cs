// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LiteDB;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfoLinks
    {
        [BsonId]
        public int Index { get; set; }
        public uint VnId { get; set; }
        public string Wikidata { get; set; }
        public string Encubed { get; set; }
        public string Renai { get; set; }
    }
}
