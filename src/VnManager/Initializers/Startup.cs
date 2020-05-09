using System;
using System.IO;
using System.IO.Abstractions;
using VnManager.Utilities;

namespace VnManager.Initializers
{
    public static class Startup
    {
        private static readonly IFileSystem fs = new FileSystem();
        public static void SetDirectories()
        {            
            if(App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) || App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))
                || App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            {
                //prog files or appdata local
                bool canReadWrite = (CheckWriteAccess(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) && CheckWriteAccess(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
                if (canReadWrite)
                {
                    App.AssetDirPath = fs.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VnManager");
                    App.ConfigDirPath = fs.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VnManager");
                    App.IsPortable = false;
                }
                else
                {
                    App.AssetDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    LogManager.Logger.Error("Could not read/write to asset/config directories");
                    AdonisUI.Controls.MessageBox.Show($"Could not read/write to user data directories!\nExiting Application", "Failed to start!", AdonisUI.Controls.MessageBoxButton.OK);
                    Environment.Exit(1);
                }
            }            
            else
            {
                //instance
                bool canReadWrite = CheckWriteAccess(App.ExecutableDirPath);
                if (canReadWrite)
                {
                    App.AssetDirPath = fs.Path.Combine(App.ExecutableDirPath, "Data");
                    App.ConfigDirPath = fs.Path.Combine(App.ExecutableDirPath, "Data");
                    App.IsPortable = true;
                }
                else
                {
                    App.AssetDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    LogManager.Logger.Error("Could not read/write to asset/config directories");
                    AdonisUI.Controls.MessageBox.Show($"Could not read/write to user data directories!\nExiting Application", "Failed to start!", AdonisUI.Controls.MessageBoxButton.OK);
                    Environment.Exit(1);
                }
            }

            CreateFolders();            
        }
        
        private static void CreateFolders()
        {
            //Assets folder ( images, logs,...)
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"res\icons\countryflags"));

            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"vndb\images\cover"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"vndb\images\screenshots"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"vndb\images\characters"));

            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"logs"));

            //Config
            fs.Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"database"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"config"));
        }

        private static bool CheckWriteAccess(string dirPath)
        {
            var testFilePath = fs.Path.Combine(dirPath, Guid.NewGuid().ToString());

            try
            {
                fs.File.WriteAllText(testFilePath, "");
                fs.File.Delete(testFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
