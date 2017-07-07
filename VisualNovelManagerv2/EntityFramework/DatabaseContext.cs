using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace VisualNovelManagerv2.EntityFramework
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configure();
        }

        public DatabaseContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
            Configure();
        }

        private void Configure()
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ModelConfiguration.Configure(modelBuilder);
            DatabaseInitializer initializer = new DatabaseInitializer(modelBuilder);
            Database.SetInitializer(initializer);

            Precision.ConfigureModelBuilder(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        }
    }
}
