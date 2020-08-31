using System;
using System.IO;
using System.IO.Abstractions;
using VnManager.Utilities;
using VnManager.Helpers;
using System.Globalization;
using System.Linq;
using LiteDB;

namespace VnManager.Initializers
{
    public static class Startup
    {
        private static readonly IFileSystem fs = new FileSystem();
        
        /// <summary>
        /// Set the default directories for the program to use (AppData)
        /// If debugging, the default directories will always be in the folder that the exe is located in
        /// </summary>
        public static void SetDirectories()
        {            
            bool canReadWrite = (CheckWriteAccess.CheckWrite(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                                 && CheckWriteAccess.CheckWrite(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
            if (canReadWrite)
            {
#if DEBUG
                App.AssetDirPath = fs.Path.Combine(App.ExecutableDirPath, "Data");
                App.ConfigDirPath = fs.Path.Combine(App.ExecutableDirPath, "Data");
#else
                App.AssetDirPath = fs.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VnManager");
                App.ConfigDirPath = fs.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VnManager");
#endif
            }
            else
            {
                App.AssetDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                App.Logger.Error("Could not read/write to asset/config directories");
                AdonisUI.Controls.MessageBox.Show($"Could not read/write to user data directories!\nExiting Application", "Failed to start!", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Exclamation);
                Environment.Exit(1);
            }

            CreateFolders();            
        }
        
        /// <summary>
        /// Create default folders for use with the program
        /// </summary>
        private static void CreateFolders()
        {
            //Assets folder ( images, logs,...)
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\cover"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\cover"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\screenshots"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\characters"));

            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"logs"));

            //Config
            fs.Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"database"));
            fs.Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"config"));
        }

        /// <summary>
        /// This should delete any log files that are a month old or older
        /// </summary>
        internal static void DeleteOldLogs()
        {
            //doesn't delete logs out of User Profile directory
            var minDate = (DateTime.Today - new TimeSpan(30, 0, 0, 0));
            if (!Directory.Exists(App.ConfigDirPath + @"\logs")) return;
            foreach (var file in Directory.GetFiles(App.ConfigDirPath + @"\logs"))
            {
                var name = Path.GetFileName(file);
                int charLoc = name.IndexOf("_", StringComparison.Ordinal);
                var subst = name.Substring(0, charLoc);

                var didParse = DateTime.TryParseExact(subst.ToCharArray(), "dd-MM-yyyy".ToCharArray(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                if (didParse == false) continue;
                if (date >= minDate) continue;
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    App.Logger.Warning(ex, "Couldn't delete log file");
                }
            }

        }

        /// <summary>
        /// This will delete old database backups that are older than 14 days.
        /// </summary>
        internal static void DeleteOldBackupDatabase()
        {
            //var dbDir = Path.Combine(App.ConfigDirPath, @"database\");
            var dbDir = Directory.EnumerateFiles(Path.Combine(App.ConfigDirPath, @"database\"), "Data_BACKUP_*.db",
                SearchOption.AllDirectories);
            var minDate = (DateTime.Today - new TimeSpan(14, 0, 0, 0));
            foreach (var file in dbDir)
            {
                var name = Path.GetFileName(file);
                var subst = name.Substring(12, 10).Replace('-', '/');
                if (DateTime.TryParse(subst, out var dbDateTime)&& dbDateTime <= minDate)
                {
                    File.Delete(name);
                }
            }
        }

    }
}
