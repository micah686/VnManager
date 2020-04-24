using System;
using System.IO;
using VnManager.Utilities;

namespace VnManager.Initializers
{
    public class Startup
    {
        public static void SetDirectories()
        {
            if(App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) || App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))
                || App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            {
                //prog files or appdata local
                bool canReadWrite = (CheckWriteAccess(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) && CheckWriteAccess(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
                if (canReadWrite)
                {
                    App.AssetDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VnManager");
                    App.ConfigDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VnManager");
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
                    App.AssetDirPath = Path.Combine(App.ExecutableDirPath, "Data");
                    App.ConfigDirPath = Path.Combine(App.ExecutableDirPath, "Data");
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

            CreatFolders();            
        }
        
        private static void CreatFolders()
        {
            //Assets folder ( images, logs,...)
            Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"res\icons\countryflags"));

            Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"vndb\images\cover"));
            Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"vndb\images\screenshots"));
            Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"vndb\images\characters"));

            Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"logs"));

            //Config
            Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"database"));
            Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"config"));
        }

        private static bool CheckWriteAccess(string dirPath)
        {
            var testFilePath = Path.Combine(dirPath, Guid.NewGuid().ToString());

            try
            {
                File.WriteAllText(testFilePath, "");
                File.Delete(testFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
