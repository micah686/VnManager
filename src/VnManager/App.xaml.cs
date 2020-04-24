using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

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

        #region AssetDirPath
        private static bool wasSetAssetDirPath = false;
        private static string _assetDirPath;
        public static string AssetDirPath
        {
            get { return _assetDirPath; }
            set
            {
                if (!wasSetAssetDirPath)
                {
                    _assetDirPath = value;
                    wasSetAssetDirPath = true;
                }
                else
                {
                    throw new InvalidOperationException("AssetDirPath can only be set once!");
                }
            }
        }
        #endregion

        #region ConfigDirPath
        private static bool wasSetConfigDirPath = false;
        private static string _configDirPath;
        public static string ConfigDirPath
        {
            get { return _configDirPath; }
            set
            {
                if (!wasSetConfigDirPath)
                {
                    _configDirPath = value;
                    wasSetConfigDirPath = true;
                }
                else
                {
                    throw new InvalidOperationException("ConfigDirPath can only be set once!");
                }
            }
        }
        #endregion

        #region IsPortable
        private static bool wasSetIsPortable = false;
        private static bool _isPortable;
        public static bool IsPortable
        {
            get { return _isPortable; }
            set
            {
                if (!wasSetIsPortable)
                {
                    _isPortable = value;
                    wasSetIsPortable = true;
                }
                else
                {
                    throw new InvalidOperationException("IsPortable can only be set once!");
                }
            }
        }
        #endregion
        public static bool IsNsfwEnabled { get; set; } = false;

    }
}
