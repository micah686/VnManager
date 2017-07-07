using System.Data.Entity;
using SQLite.CodeFirst;
using VisualNovelManagerv2.EntityFramework.Entity;

namespace VisualNovelManagerv2.EntityFramework
{
    public class DatabaseInitializer:SqliteDropCreateDatabaseWhenModelChanges<DatabaseContext>
    {
        public DatabaseInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, typeof(CustomHistory))
        { }

        protected override void Seed(DatabaseContext context)
        {
            // Seed core data here if necessary
            // base.Seed(context);
        }
    }
}
