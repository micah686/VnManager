using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace VisualNovelManagerv2
{
    public class StartupValidate
    {
        readonly string _directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public void CreateFolders()
        {
            Directory.CreateDirectory(_directoryPath + @"\Data\config");
            Directory.CreateDirectory(_directoryPath + @"\Data\Database");
            Directory.CreateDirectory(_directoryPath + @"\Data\images\character");
            Directory.CreateDirectory(_directoryPath + @"\Data\images\cover");
            Directory.CreateDirectory(_directoryPath + @"\Data\images\screenshots");
            Directory.CreateDirectory(_directoryPath + @"\Data\images\thumbs");
            Directory.CreateDirectory(_directoryPath + @"\Data\images\vnlist");
            Directory.CreateDirectory(_directoryPath + @"\Data\libs\");
            Directory.CreateDirectory(_directoryPath + @"\Data\res\country_flags");
            Directory.CreateDirectory(_directoryPath + @"\Data\res\icons");
        }

        public void CheckForDatabase()
        {
            string dbPath =
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Data\Database\Database.db");

            if (!File.Exists(dbPath))
            {
                CreateDatabase();
            }

            if (File.Exists(dbPath))
            {
                bool isValid = DbConnection();
                if (isValid == true) return;
                File.Delete(dbPath);
                CreateDatabase();
            }
        }

        private static bool DbConnection()
        {
            //TODO:try to check for the tables as well
            try
            {
                using (
                    SQLiteConnection conn =
                        new SQLiteConnection(@"Data Source=|DataDirectory|\Database\Database.db;" + "Version=3;"))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (SQLiteException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        void CreateDatabase()
        {

            string connectionString = @"Data Source=|DataDirectory|\Data\Database\Database.db;" +
                                      "Version=3;" + "Pooling=True;" + "Max Pool Size=5;" + "Page Size=4096;" +
                                      "Cache Size=4000;"+ "PRAGMA foreign_keys = ON;";
            //string connectionString = $"Data Source={Path.Combine(this._directoryPath, @"\Data\Database\Database.db")};" +
            //                          "Version=3;" + "Pooling=True;" + "Max Pool Size=5;" + "Page Size=4096;" +
            //                          "Cache Size=4000;";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                //SQLiteConnection.CreateFile(Path.Combine(this._directoryPath, @"\Data\Database\Database.db"));
                conn.Open();

                using (SQLiteCommand query = conn.CreateCommand())
                {
                    query.CommandText =
                        @"BEGIN;

                        CREATE TABLE IF NOT EXISTS VnInfo (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            Title text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            Released text DEFAULT NULL,
                            Languages text DEFAULT NULL,
                            Original_Language text DEFAULT NULL,
                            Platforms text DEFAULT NULL,
                            Aliases text DEFAULT NULL,
                            Length text DEFAULT NULL,
                            Description text DEFAULT NULL,
                            ImageLink text DEFAULT NULL,
                            ImageNsfw text DEFAULT NULL,
                            Popularity numeric(5,2) DEFAULT NULL,
                            Rating numeric(4,2) DEFAULT NULL,
                            PRIMARY KEY (PK_Id),
                            CONSTRAINT fkey0 FOREIGN KEY (PK_Id) REFERENCES VnInfoLinks(PK_Id),
                            CONSTRAINT fkey1 FOREIGN KEY (PK_Id) REFERENCES VnAnime(PK_Id),
                            CONSTRAINT fkey2 FOREIGN KEY (PK_Id) REFERENCES VnRelations(PK_Id),
                            CONSTRAINT fkey3 FOREIGN KEY (PK_Id) REFERENCES VnTags(PK_Id),
                            CONSTRAINT fkey4 FOREIGN KEY (PK_Id) REFERENCES VnScreens(PK_Id),
                            CONSTRAINT fkey5 FOREIGN KEY (PK_Id) REFERENCES VnInfoStaff(PK_Id)
                        );

                        CREATE TABLE VnInfoLinks (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            Wikipedia integer DEFAULT NULL,
                            Encubed text DEFAULT NULL,
                            Renai text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnAnime (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            AniDbId integer DEFAULT NULL,
                            AnnId integer DEFAULT NULL,
                            AniNfoId text DEFAULT NULL,
                            Title_Eng text DEFAULT NULL,
                            Title_Jpn text DEFAULT NULL,
                            Year integer DEFAULT NULL,
                            Anime_Type text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnRelations (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            RelationId integer DEFAULT NULL,
                            Relation text DEFAULT NULL,
                            Title text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            Official text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnTags (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            TagId integer DEFAULT NULL,
                            TagName text DEFAULT NULL,
                            Score numeric(5,2) DEFAULT NULL,
                            Spoiler integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnScreens (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            ImageUrl text DEFAULT NULL,
                            ReleaseId text DEFAULT NULL,
                            Nsfw text DEFAULT NULL,
                            Height integer DEFAULT NULL,
                            Width integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnInfoStaff (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            StaffId integer DEFAULT NULL,
                            AliasId integer DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            Role text DEFAULT NULL,
                            Note integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnRelease (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            ReleaseId integer DEFAULT NULL,
                            Title text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            Released text DEFAULT NULL,
                            ReleaseType text DEFAULT NULL,
                            Patch text DEFAULT NULL,
                            Freeware text DEFAULT NULL,
                            Doujin text DEFAULT NULL,
                            Languages text DEFAULT NULL,
                            Website text DEFAULT NULL,
                            Notes text DEFAULT NULL,
                            MinAge integer DEFAULT NULL,
                            Gtin integer DEFAULT NULL,
                            Catalog text DEFAULT NULL,
                            Platforms text DEFAULT NULL,
                            PRIMARY KEY (PK_Id),
                            CONSTRAINT fkey0 FOREIGN KEY (PK_Id) REFERENCES VnReleaseMedia(PK_Id),
                            CONSTRAINT fkey1 FOREIGN KEY (PK_Id) REFERENCES VnReleaseVn(PK_Id),
                            CONSTRAINT fkey2 FOREIGN KEY (PK_Id) REFERENCES VnReleaseProducers(PK_Id)
                        );

                        CREATE TABLE VnReleaseMedia (
                            PK_Id integer NOT NULL,
                            ReleaseId integer DEFAULT NULL,
                            Medium text DEFAULT NULL,
                            Quantity integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnReleaseVn (
                            PK_Id integer NOT NULL,
                            ReleaseId integer DEFAULT NULL,
                            VnId integer DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnReleaseProducers(
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            ProducerId integer DEFAULT NULL,
                            Developer text DEFAULT NULL,
                            Publisher text DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            ProducerType text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnStaff (
                            PK_Id integer NOT NULL,
                            StaffId integer DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            Gender text DEFAULT NULL,
                            Language text DEFAULT NULL,
                            Description text DEFAULT NULL,
                            MainAlias integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id),
                            CONSTRAINT fkey0 FOREIGN KEY (PK_Id) REFERENCES VnStaffAliases (PK_Id),
                            CONSTRAINT fkey1 FOREIGN KEY (PK_Id) REFERENCES VnStaffLinks (PK_Id)
                        );

                        CREATE TABLE VnStaffAliases ( 
                            PK_Id integer NOT NULL, 
                            StaffId integer DEFAULT NULL, 
                            AliasId integer DEFAULT NULL, 
                            Name text DEFAULT NULL, 
                            Original text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnStaffLinks ( 
                            PK_Id integer NOT NULL ,
                            StaffId integer DEFAULT NULL,
                            Homepage text DEFAULT NULL,
                            Wikipedia text DEFAULT NULL,
                            Twitter text DEFAULT NULL,
                            AniDb text DEFAULT NULL, 
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE VnCharacter ( 
                            PK_Id integer NOT NULL ,
                            VnId integer DEFAULT NULL,
                            CharacterId integer DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text Default NULL,
                            Gender text DEFAULT NULL, 
                            BloodType text DEFAULT NULL,
                            Birthday text DEFAULT NULL,
                            Aliases text DEFAULT NULL,
                            Description text DEFAULT NULL,
                            ImageLink text DEFAULT NULL,
                            Bust integer DEFAULT NULL,
                            Waist integer DEFAULT NULL,
                            Hip integer DEFAULT NULL,
                            Height integer DEFAULT NULL,
                            Weight integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id),
                            CONSTRAINT fkey0 FOREIGN KEY (PK_Id) REFERENCES VnCharacterTraits (PK_Id),
                            CONSTRAINT fkey1 FOREIGN KEY (PK_Id) REFERENCES VnCharacterVns (PK_Id)
                        );

                        CREATE TABLE VnCharacterTraits (
                            PK_Id integer NOT NULL DEFAULT NULL,
                            CharacterId integer DEFAULT NULL,
                            SpoilerLevel integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );
                        
                        CREATE TABLE VnCharacterVns (
                            PK_Id integer NOT NULL DEFAULT NULL,
                            CharacterId integer DEFAULT NULL,
                            VnId integer DEFAULT NULL,
                            ReleaseId integer DEFAULT NULL,
                            SpoilerLevel integer DEFAULT NULL,
                            Role text DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );
                       
                        CREATE TABLE VnProducer (
                            PK_Id integer NOT NULL,
                            ProducerId integer DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            ProducerType text DEFAULT NULL,
                            Language text DEFAULT NULL,
                            Links text DEFAULT NULL,
                            Aliases text DEFAULT NULL,
                            Description text DEFAULT NULL,
                            PRIMARY KEY (PK_Id),
                            CONSTRAINT fkey0 FOREIGN KEY (PK_Id) REFERENCES VnProducerRelations (PK_id),
                            CONSTRAINT fkey1 FOREIGN KEY (PK_Id) REFERENCES VnProducerLinks (PK_id)
                        );

                        CREATE TABLE VnProducerRelations (
                            PK_id integer NOT NULL,
                            RelationId integer DEFAULT NULL,
                            ProducerId integer DEFAULT NULL,
                            Relation text DEFAULT NULL,
                            Name text DEFAULT NULL,
                            Original text DEFAULT NULL,
                            PRIMARY KEY (PK_id) 
                        );

                        CREATE TABLE VnProducerLinks (
                            PK_id integer NOT NULL,
                            ProducerId integer DEFAULT NULL,
                            Homepage text DEFAULT NULL,
                            Wikipedia text DEFAULT NULL,
                            PRIMARY KEY (PK_id) 
                        );

                        CREATE TABLE VnUserData (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            VnName text DEFAULT NULL,
                            ExePath text DEFAULT NULL,
                            IconPath text DEFAULT NULL,
                            LastPlayed text DEFAULT NULL,
                            SecondsPlayed text DEFAULT NULL,
                            Categories integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id),
                            CONSTRAINT fkey0 FOREIGN KEY (PK_Id) REFERENCES VnUserDataCategories (PK_Id)
                        );

                        CREATE TABLE VnUserDataCategories (
                            PK_Id integer NOT NULL,
                            VnId integer DEFAULT NULL,
                            Category integer DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );

                        CREATE TABLE Categories (
                            PK_Id integer NOT NULL,
                            Category text  DEFAULT NULL,
                            PRIMARY KEY (PK_Id) 
                        );


                        CREATE TABLE IF NOT EXISTS DlSiteCircle (
                            CircleID integer PRIMARY KEY,
                            RGCode varchar(255) DEFAULT NULL,
                            Name varchar(255) UNIQUE
                        );
                    
                        CREATE TABLE IF NOT EXISTS DlSiteGame (
                            GameID integer PRIMARY KEY,
                            RJCode varchar(255) DEFAULT NULL,
                            Title varchar(255) DEFAULT NULL,
                            FolderPath varchar(255) DEFAULT NULL,
                            Rating tinyint DEFAULT NULL,
                            DLSRating tinyint DEFAULT NULL,
                            ReleaseDate datetime DEFAULT NULL,
                            AddedDate datetime DEFAULT NULL,
                            LastPlayedDate datetime DEFAULT NULL,
                            TimesPlayed integer NOT NULL,
                            SecondsPlayed integer NOT NULL,
                            CircleID integer DEFAULT NULL, --REFERENCES DlSiteCircle(CircleID), --Foreign keys are ignored by SQLite
                            Category varchar(255) DEFAULT NULL,
                            Tags text DEFAULT NULL,
                            Comments text DEFAULT NULL,
                            RunWithAgth tinyint DEFAULT NULL,
                            AgthParameters text DEFAULT NULL,
                            Size integer DEFAULT NULL,
                            DLSiteFlags smallint NOT NULL,
                            IsRpgMakerGame tinyint NOT NULL,
                            WolfRpgMakerVersion varchar(255) DEFAULT NULL,
                            UseCustomResolution tinyint NOT NULL,
                            ResolutionWidth smallint DEFAULT NULL,
                            ResolutionHeight smallint DEFAULT NULL
                        );

                        CREATE TABLE IF NOT EXISTS DlSiteImage (
                            ImageID integer PRIMARY KEY,
                            ImagePath varchar(255),
                            IsListImage tinyint NOT NULL,
                            IsCoverImage tinyint NOT NULL,
                            GameID integer NOT NULL
                                --REFERENCES DlsiteGame(GameID)
                        );

                        PRAGMA user_version;

                        COMMIT;";
                    long version = (long)query.ExecuteScalar();

                    if (version < 1)
                    {
                        query.CommandText = "PRAGMA user_version = 1;";
                        query.ExecuteNonQuery();
                    }

                    if (version < 2)
                    {
                        //Add the language column to the game table and set its value to Japanese for all games with an RJCode
                        query.CommandText = @"BEGIN;
                                              PRAGMA user_version = 2;
                                              ALTER TABLE DLSiteGame ADD Language varchar(255);
                                              UPDATE DLSiteGame SET Language = 'Japanese' WHERE RJCode IS NOT NULL;
                                              COMMIT;";
                        query.ExecuteNonQuery();

                    }
                    if (version < 3)
                    {
                        //Remove all new lines from game titles that begin with a new line
                        query.CommandText = @"BEGIN;
                                              PRAGMA user_version = 3;
                                              UPDATE DLSiteGame SET Title = replace(Title, x'0a', '') WHERE Title LIKE x'0a' || '%';
                                              COMMIT;";
                        query.ExecuteNonQuery();
                    }
                }
            }
        }

        private static void BrowseExe()
        {
            OpenFileDialog dialogBrowse = new OpenFileDialog()
            {
                FileName = null,
                DefaultExt = ".exe",
                Filter = "Applications (*.exe)|*.exe;",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true,
                Title = "Browse for Visual Novel Execuatable"
            };

            bool? result = dialogBrowse.ShowDialog();
        }

        private void ValidateExe()
        {
            if (File.Exists(@"D:\Visual Novels\file.exe"))
            {
                var twobytes = new byte[2];
                using (FileStream filestream = File.Open(@"D:\Visual Novels\file.exe", FileMode.Open))
                {
                    filestream.Read(twobytes, 0, 2);
                }
                switch (Encoding.UTF8.GetString(twobytes))
                {
                    case "MZ":
                        break;
                    case "ZM":
                        break;
                    default:
                        Console.Write("Invalid Exe");
                        break;
                }
            }
        }

    }
}
