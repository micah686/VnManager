using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using VnManager.Models.Settings;
using VnManager.Utilities;

namespace VnManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Assembly Assembly { get; } = typeof(App).Assembly;
        private const int FieldCount = 3;
        public static string VersionString { get; } = Assembly.GetName().Version?.ToString(FieldCount);

        public static string ExecutableDirPath { get; } = AppDomain.CurrentDomain.BaseDirectory!;

        #region StartupLockout
        private static bool _wasSetStartupLockout = false;
        private static bool _startupLockout;
        /// <summary>
        /// Lockout variable used to prevent changing certain configurations after the application has started up
        /// </summary>
        public static bool StartupLockout
        {
            get => _startupLockout;
            set
            {
                if (!_wasSetStartupLockout)
                {
                    _startupLockout = value;
                    _wasSetStartupLockout = true;
                }
                else
                {
                    throw new InvalidOperationException("Value cannot be set after Startup has finished");
                }
            }
        }
        #endregion

        #region AssetDirPath 
        private static string _assetDirPath;
        /// <summary>
        /// Directory for saving assets like images, logs,...
        /// </summary>
        public static string AssetDirPath
        {
            get => _assetDirPath;
            set
            {
                if (!StartupLockout)
                {
                    _assetDirPath = value;
                }
                else
                {
                    throw new InvalidOperationException("AssetDirPath can only be set once!");
                }
            }
        }
        #endregion

        #region ConfigDirPath
        private static string _configDirPath;
        /// <summary>
        /// Configuration directory path, where the userSettings, database,... are stored
        /// </summary>
        public static string ConfigDirPath
        {
            get => _configDirPath;
            set
            {
                if (!StartupLockout)
                {
                    _configDirPath = value;
                }
                else
                {
                    throw new InvalidOperationException("ConfigDirPath can only be set once!");
                }
            }
        }
        #endregion


        #region Logger
        private static ILogger _logger= LogManager.Logger;
        /// <summary>
        /// Logger instance used for writing log files
        /// </summary>
        public static ILogger Logger
        {
            get => _logger ?? LogManager.Logger;
            set
            {
                if (!StartupLockout)
                {
                    _logger = value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot set Logger after Startup");
                }
            }
        }
        #endregion

        /// <summary>
        /// Current user settings for the application
        /// </summary>
        public static UserSettings UserSettings { get; set; }

        /// <summary>
        /// Resource Manager used to retrieve strings from the .resx files
        /// </summary>
        public static readonly ResourceManager ResMan = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Database connection string without the password. Use CredentialManager to get the password
        /// </summary>
        public static string GetDbStringWithoutPass => $"Filename={Path.Combine(ConfigDirPath, DbPath)};Password=";

        /// <summary>
        /// Static instance of the Statusbar, so any method can write values to it
        /// </summary>
        //public static StatusBarViewModel StatusBar { get; set; }

        public static bool DidDownloadTagTraitDump { get; set; }

        /// <summary>
        /// Credential name for the database encryption
        /// </summary>
        internal const string CredDb = "VnManager.DbEnc";
        /// <summary>
        /// Credential name for the file encryption
        /// </summary>
        internal const string CredFile = "VnManager.FileEnc";
        /// <summary>
        /// Relative path name for the database
        /// </summary>
        internal const string DbPath = @"database\Data.db";
        /// <summary>
        /// Password to encrypt/decrypt the database dump for importing/exporting
        /// </summary>
        internal const string ImportExportDbKey = "VnManager!Import#Key_33087@Unlock";

    }
}
