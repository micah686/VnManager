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
