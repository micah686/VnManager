using MvvmDialogs;
using Stylet;
using StyletIoC;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using VnManager.Helpers;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Conductor<Screen>
    {
        private static readonly ResourceManager Rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
        public StatusBarViewModel StatusBarPage { get; set; }

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;

        private int _windowButtonPressedCounter = 0;

        public static RootViewModel Instance { get; private set; }
        public string WindowTitle { get; } = string.Format($"{Rm.GetString("ApplicationTitle")} {App.VersionString}");

        #region SettingsPressed
        private bool _isSettingsPressed;
        public bool IsSettingsPressed
        {
            get => _isSettingsPressed;
            set
            {
                if(_windowButtonPressedCounter != 0 && value == true)
                {
                    SetAndNotify(ref _isSettingsPressed, false);
                    return;
                }
                SetAndNotify(ref _isSettingsPressed, value);
                if (_isSettingsPressed == true)
                {
                    _windowButtonPressedCounter += 1;
                    SettingsIconColor = System.Windows.Media.Brushes.LimeGreen;
                    ActivateSettingsClick();
                }
                else
                {
                    _windowButtonPressedCounter -= 1;
                    var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
                    SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);
                    ActivateMainClick();
                }
            }
        }

        public System.Windows.Media.Brush SettingsIconColor { get; set; }
        #endregion

        #region DebugPressed
        private bool _debugPressed;
        public bool DebugPressed
        {
            get => _debugPressed;
            set
            {
                if(_windowButtonPressedCounter != 0 && value == true)
                {
                    SetAndNotify(ref _debugPressed, false);
                    return;
                }
                SetAndNotify(ref _debugPressed, value);
                if(_debugPressed == true)
                {
                    _windowButtonPressedCounter += 1;
                    DebugClick();
                }
                else
                {
                    _windowButtonPressedCounter -= 1;
                    ActivateMainClick();
                }
            }
        }

        #endregion


        public RootViewModel(IContainer container, IWindowManager windowManager)
        {
            Instance = this;
            _container = container;
            _windowManager = windowManager;


             

        }

        protected override void OnViewLoaded()
        {

            if (!IsNormalStart())
            {
                App.UserSettings = UserSettingsHelper.ReadUserSettings();
                var auth = _container.Get<SetEnterPasswordViewModel>();
                var isAuth = _windowManager.ShowDialog(auth);
                
                if (isAuth == true)
                {
                    var maingrid = _container.Get<MainGridViewModel>();
                    ActivateItem(maingrid);
                    StatusBarPage = _container.Get<StatusBarViewModel>();
                    var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
                    SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);
                }
                else
                {
                    Environment.Exit(0);//closed auth window
                }
            }
            else
            {
                var maingrid = _container.Get<MainGridViewModel>();
                ActivateItem(maingrid);
                StatusBarPage = _container.Get<StatusBarViewModel>();
                var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
                SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);
            }




        }

        private bool IsNormalStart()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.json");
            if (!File.Exists(configFile)) return false;
            if (!UserSettingsHelper.ValidateConfigFile()) return false;
            if (!File.Exists(Path.Combine(App.ConfigDirPath, @"secure\secrets.store")) ||
                !File.Exists(Path.Combine(App.ConfigDirPath, @"secure\secrets.key"))) return false;
            App.UserSettings = UserSettingsHelper.ReadUserSettings();
            var useEncryption = App.UserSettings.EncryptionEnabled;
            return !useEncryption;
        }


        public void ActivateSettingsClick()
        {
            var vm = _container.Get<SettingsViewModel>();
            ActivateItem(vm);
        }

        public void ActivateMainClick()
        {
            var vm = _container.Get<MainGridViewModel>();
            ActivateItem(vm);
        }


        public void DebugClick()
        {
            var vm = _container.Get<DebugViewModel>();
            ActivateItem(vm);
        }

    }
}
