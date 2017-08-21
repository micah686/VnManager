using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VisualNovelManagerv2.EF.Entity.VnCharacter;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnProducer;
using VisualNovelManagerv2.EF.Entity.VnRelease;
using VisualNovelManagerv2.EF.Entity.VnStaff;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VisualNovelManagerv2.EF.Entity.VnUserList;


namespace VisualNovelManagerv2.EF.Context
{
    public class DatabaseContext: DbContext
    {
        static DatabaseContext()
        {
            using (var database = new DatabaseContext())
            {
                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                database.Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            optionsBuilder.UseSqlite($@"Filename={directoryPath}\Data\Database\Database.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }

        internal DbSet<Categories> Categories { get; set; }
        internal DbSet<VnCharacter> VnCharacter { get; set; }
        internal DbSet<VnCharacterTraits> VnCharacterTraits { get; set; }
        internal DbSet<VnCharacterVns> VnCharacterVns { get; set; }
        internal DbSet<VnInfo> VnInfo { get; set; }
        internal DbSet<VnInfoAnime> VnInfoAnime { get; set; }
        internal DbSet<VnInfoLinks> VnInfoLinks { get; set; }
        internal DbSet<VnInfoRelations> VnInfoRelations { get; set; }
        internal DbSet<VnInfoScreens> VnInfoScreens { get; set; }
        internal DbSet<VnInfoStaff> VnInfoStaff { get; set; }
        internal DbSet<VnInfoTags> VnInfoTags { get; set; }
        internal DbSet<VnProducer> VnProducer { get; set; }
        internal DbSet<VnProducerLinks> VnProducerLinks { get; set; }
        internal DbSet<VnProducerRelations> VnProducerRelations { get; set; }
        internal DbSet<VnRelease> VnRelease { get; set; }
        internal DbSet<VnReleaseMedia> VnReleaseMedia { get; set; }
        internal DbSet<VnReleaseProducers> VnReleaseProducers { get; set; }
        internal DbSet<VnReleaseVn> VnReleaseVn { get; set; }
        internal DbSet<VnStaff> VnStaff { get; set; }
        internal DbSet<VnStaffAliases> VnStaffAliases { get; set; }
        internal DbSet<VnStaffLinks> VnStaffLinks { get; set; }
        internal DbSet<VnTagData> VnTagData { get; set; }
        internal DbSet<VnTraitData> VnTraitData { get; set; }
        internal DbSet<VnUserData> VnUserData { get; set; }
        internal DbSet<VnUserDataCategories> VnUserDataCategories { get; set; }
        internal DbSet<VnVoteList> VnVoteList { get; set; }
        internal DbSet<VnVisualNovelList> VnVisualNovelList { get; set; }
        internal DbSet<VnWishList> VnWishList { get; set; }
    }
}
