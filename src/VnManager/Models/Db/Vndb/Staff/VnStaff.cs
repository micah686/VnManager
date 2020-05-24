using System.Collections.ObjectModel;
using LiteDB;
using VndbSharp.Models.Common;
using VndbSharp.Models.Staff;

namespace VnManager.Models.Db.Vndb.Staff
{
    public class VnStaff
    {
        [BsonId]
        public int Index { get; set; }
        public int? StaffId { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public Gender? Gender { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string MainAliasId { get; set; }
        public virtual VnStaffAliases VnStaffAliases { get; set; }
        public virtual VnStaffLinks VnStaffLinks { get; set; }
        public ReadOnlyCollection<StaffVns> StaffVns { get; set; }
        public ReadOnlyCollection<StaffVoiced> StaffVoicedCollection { get; set; }
    }
}
