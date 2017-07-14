using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.EntityFramework;
using VisualNovelManagerv2.EntityFramework.Entity.VnInfo;

namespace VisualNovelManagerv2
{
    public class StartupValidate
    {
        public void CreateFolders()
        {
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\config");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Database");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\characters");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\cover");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\screenshots");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\vnlist");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\libs\");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons\country_flags");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons\gender");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons\assorted");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons\statusbar");
        }

        public void CheckForDatabase()
        {
            string dbPath =
                Path.Combine(Globals.DirectoryPath + @"\Data\Database\Database.db");

            if (!File.Exists(dbPath))
            {
                CreateDatabase();
            }

            if (File.Exists(dbPath))
            {
                bool isValid = DbConnection();
                if (isValid == true) return;
                File.Move(dbPath, Path.Combine(dbPath+".corrupt"));
                CreateDatabase();
            }
        }

        static bool DbConnection()
        {
            //TODO:try to check for the tables as well
            //TODO: validate aginst EF model once ADO.net provider is released and get igrations working
            //needs custom string without pooling to be able to delete the database
            using (SQLiteConnection db = new SQLiteConnection(@"Data Source=|DataDirectory|\Data\Database\Database.db;Version=3;"))
            {
                try
                {
                    db.Open();
                    using (SQLiteTransaction transaction = db.BeginTransaction())
                    {
                        transaction.Rollback();
                    }
                }
                catch (SQLiteException ex)
                {
                    db.Close();
                    DebugLogging.WriteDebugLog(ex);
                    return false;
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    return false;
                }
            }
            return true;
        }

        void CreateDatabase()
        {
            using (var context = new DatabaseContext("Database"))
            {                
                //context.Set<VnInfo>().Add(new VnInfo
                //{
                //    VnId = 32,
                //    Title = "sampleEntry",
                //    Popularity = 15.3
                //});

                //context.SaveChanges();
                context.Dispose();
            }
        }        

    }
}
