// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LiteDB;

namespace VnManager.Models.Db.User
{
    public class UserDataCategories
    {
        [BsonId]
        public int Index { get; set; }
        public string CategoryName { get; set; }
    }
}
