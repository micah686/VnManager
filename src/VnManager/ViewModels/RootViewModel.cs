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


            // StatusBarPage = _container.Get<StatusBarViewModel>();


            //var maingrid = _container.Get<MainGridViewModel>();
            //ActivateItem(maingrid);
            //var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
            //SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);




           

        }

        protected override void OnViewLoaded()
        {

            if (!IsNormalStart())
            {
                var auth = _container.Get<SetEnterPasswordViewModel>();
                var isAuth = _windowManager.ShowDialog(auth);
                if (isAuth == true)
                {
                    var maingrid = _container.Get<MainGridViewModel>();
                    ActivateItem(maingrid);
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
                var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
                SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);
            }




        }

        private bool IsNormalStart()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.xml");
            if (!File.Exists(configFile)) return false;
            if (!ValidateXml.IsValidXml(configFile)) return false;

            SettingsViewModel.LoadUserSettingsStatic();
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
