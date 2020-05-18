using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using VnManager.Utilities;

namespace VnManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Assembly Assembly { get; } = typeof(App).Assembly;
        public static string VersionString { get; } = Assembly.GetName().Version.ToString(3);

        public static string ExecutableDirPath { get; } = AppDomain.CurrentDomain.BaseDirectory!;

        #region StartupLockout
        private static bool wasSetStartupLockout = false;
        private static bool _startupLockout;
        public static bool StartupLockout
        {
            get { return _startupLockout; }
            set
            {
                if (!wasSetStartupLockout)
                {
                    _startupLockout = value;
                    wasSetStartupLockout = true;
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
        public static string AssetDirPath
        {
            get { return _assetDirPath; }
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
        public static string ConfigDirPath
        {
            get { return _configDirPath; }
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

        #region IsPortable
        private static bool _isPortable;
        public static bool IsPortable
        {
            get { return _isPortable; }
            set
            {
                if (!StartupLockout)
                {
                    _isPortable = value;
                }
                else
                {
                    throw new InvalidOperationException("IsPortable can only be set once!");
                }
            }
        }
        #endregion

        public static bool IsNsfwEnabled { get; set; } = false;

        public static ILogger Logger { get; } = LogManager.Logger;
    }
}
