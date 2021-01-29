// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LiteDB;
using VndbSharp.Models.Common;

namespace VnManager.Models.Db.Vndb.Main
{
    public class VnInfo
    {
        [BsonId]
        public int Index { get; set; }
        public uint VnId { get; set; }
        public string Title { get; set; }
        public string Original { get; set; }
        public string Released { get; set; }
        public string Languages { get; set; }
        public string OriginalLanguages { get; set; }
        public string Platforms { get; set; }
        public string Aliases { get; set; }
        public string Length { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public ImageRating ImageRating { get; set; }
        public double? Popularity { get; set; }
        public double Rating { get; set; }
    }
}
