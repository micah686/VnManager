// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LiteDB;

namespace VnManager.Models.Db.Vndb.TagTrait
{
    public class VnTraitData
    {
        [BsonId]
        public int Index { get; set; }
        public uint TraitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMeta { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsApplicable { get; set; }
        public uint Characters { get; set; }
        public string Aliases { get; set; }
        public uint[] Parents { get; set; }
    }
}
