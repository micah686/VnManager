using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Model.Settings;
using DatabaseContext = VisualNovelManagerv2.EF.Context.DatabaseContext;

namespace VisualNovelManagerv2
{
    public class StartupValidate
    {
        public ICommand CreateFoldersCommand => new RelayCommand(CreateFolders);
        public ICommand CheckForDatabaseCommand => new RelayCommand(CheckForDatabase);
        public ICommand CheckXmlConfigCommand => new RelayCommand(CheckXmlConfig);
        public ICommand LoadXmlSettingsCommand => new RelayCommand(LoadXmlSettings);

        private void CreateFolders()
        {
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\config");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Database");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\characters");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\cover");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\screenshots");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\vnlist");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\images\userlist");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\libs\");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\res\icons\country_flags");
        }

        private void CheckForDatabase()
        {
            string dbPath =
                Path.Combine(Globals.DirectoryPath + @"\Data\Database\Database.db");
            CreateDatabase();
            if (!File.Exists(dbPath))
            {
                CreateDatabase();
            }
        }

        void CreateDatabase()
        {
            using (var db = new DatabaseContext())
            {
                db.Dispose();
            }
        }

        private void CheckXmlConfig()
        {
            UserSettings userSettings = new UserSettings();
            if (!File.Exists(Globals.DirectoryPath + @"/Data/config/config.xml"))
            {
                userSettings.NsfwEnabled = false;
                userSettings.MaxSpoilerLevel = 0;
                ModifyUserSettings.SaveUserSettings(userSettings);
            }
            UserSettings settings = ModifyUserSettings.LoadUserSettings();
            if (settings != null) return;
            File.Delete(Globals.DirectoryPath + @"/Data/config/config.xml");            
            userSettings.NsfwEnabled = false;
            userSettings.MaxSpoilerLevel = 0;
            ModifyUserSettings.SaveUserSettings(userSettings);            
        }

        private void LoadXmlSettings()
        {
            Globals.NsfwEnabled = ModifyUserSettings.LoadUserSettings().NsfwEnabled;
            Globals.MaxSpoiler = ModifyUserSettings.LoadUserSettings().MaxSpoilerLevel;
        }

    }
}
