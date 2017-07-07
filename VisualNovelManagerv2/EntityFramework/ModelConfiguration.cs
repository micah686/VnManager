using System.Data.Entity;
using VisualNovelManagerv2.EntityFramework.Entity.VnCharacter;
using VisualNovelManagerv2.EntityFramework.Entity.VnInfo;
using VisualNovelManagerv2.EntityFramework.Entity.VnOther;
using VisualNovelManagerv2.EntityFramework.Entity.VnProducer;
using VisualNovelManagerv2.EntityFramework.Entity.VnRelease;
using VisualNovelManagerv2.EntityFramework.Entity.VnStaff;
using VisualNovelManagerv2.EntityFramework.Entity.VnTagTrait;

namespace VisualNovelManagerv2.EntityFramework
{
    public class ModelConfiguration
    {
        public static void Configure(DbModelBuilder modelBuilder)
        {
            ConfigureVnCharacterEntity(modelBuilder);
            ConfigureVnInfoEntity(modelBuilder);
            ConfigureVnOther(modelBuilder);
            ConfigureVnProducerEntity(modelBuilder);
            ConfigureVnReleaseEntity(modelBuilder);
            ConfigureVnStaffEntity(modelBuilder);
            ConfigureVnTagTraitEntity(modelBuilder);


        }

        private static void ConfigureVnCharacterEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VnCharacter>();
            modelBuilder.Entity<VnCharacterTraits>();
            modelBuilder.Entity<VnCharacterVns>();
        }

        private static void ConfigureVnInfoEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VnInfo>();
            modelBuilder.Entity<VnInfoAnime>();
            modelBuilder.Entity<VnInfoLinks>();
            modelBuilder.Entity<VnInfoRelations>();
            modelBuilder.Entity<VnInfoScreens>();
            modelBuilder.Entity<VnInfoStaff>();
            modelBuilder.Entity<VnInfoTags>();
        }

        private static void ConfigureVnOther(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categories>();
            modelBuilder.Entity<VnUserData>();
            modelBuilder.Entity<VnUserDataCategories>();
        }

        private static void ConfigureVnProducerEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VnProducer>();
            modelBuilder.Entity<VnProducerLinks>();
        }        

        private static void ConfigureVnReleaseEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VnRelease>();
            modelBuilder.Entity<VnReleaseMedia>();
            modelBuilder.Entity<VnReleaseProducers>();
            modelBuilder.Entity<VnReleaseVn>();
        }

        private static void ConfigureVnStaffEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VnStaff>();
            modelBuilder.Entity<VnStaffAliases>();
            modelBuilder.Entity<VnStaffLinks>();
        }

        private static void ConfigureVnTagTraitEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VnTagData>();
            modelBuilder.Entity<VnTraitData>();
        }

    }
}
