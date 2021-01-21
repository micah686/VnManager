using Stylet;
using StyletIoC;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using VnManager.ViewModels.UserControls;

namespace VnManager.ViewModels
{
    /// <summary>
    /// Interface for creating the MainGrid
    /// </summary>
    public interface IMainGridViewModelFactory
    {
        MainGridViewModel CreateMainGridViewModel();
    }
    /// <summary>
    /// Interface for creating the Settings View
    /// </summary>
    public interface ISettingsViewModelFactory
    {
        SettingsViewModel CreateSettingsViewModel();
    }
    /// <summary>
    /// Interface for creating thr DebugView
    /// </summary>
    public interface IDebugViewModelFactory
    {
        DebugViewModel CreateDebugViewModel();
    }
    
    public class RootViewModel: Conductor<Screen>
    {
        public StatusBarViewModel StatusBarPage { get; set; }

        private readonly IContainer _container;
        private readonly IMainGridViewModelFactory _mainGridVmFactory;
        private readonly ISettingsViewModelFactory _settingsVmFactory;
        private readonly IDebugViewModelFactory _debugVmFactory;

        private int _windowButtonPressedCounter = 0;

        public static RootViewModel Instance { get; private set; }

        public static string WindowTitle => FormatWindowTitle();

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


#if DEBUG
        public RootViewModel(IContainer container, IWindowManager windowManager, IMainGridViewModelFactory mainGridFactory, ISettingsViewModelFactory settingsFactory,
            IDebugViewModelFactory debugFactory)
        {
            Instance = this;
            _container = container;
            _mainGridVmFactory = mainGridFactory;
            _settingsVmFactory = settingsFactory;
            _debugVmFactory = debugFactory;
            App.StatusBar = _container.Get<StatusBarViewModel>();
            _ = new Initializers.Startup(_container, windowManager);

        }
#else
        public RootViewModel(IContainer container, IWindowManager windowManager, IMainGridViewModelFactory mainGridFactory, ISettingsViewModelFactory settingsFactory)
        {
            Instance = this;
            _container = container;
            _windowManager = windowManager;
            _mainGridVmFactory = mainGridFactory;
            _settingsVmFactory = settingsFactory;
            App.StatusBar = _container.Get<StatusBarViewModel>();

        }
#endif



        protected override void OnViewLoaded()
        {
            var mainGridVm = _mainGridVmFactory.CreateMainGridViewModel();
            ActivateItem(mainGridVm);
            StatusBarPage = _container.Get<StatusBarViewModel>();
            var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
            SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);
            
        }

        /// <summary>
        /// Creates a formatted window title
        /// </summary>
        /// <returns></returns>
        private static string FormatWindowTitle()
        {
            var appName = App.ResMan.GetString("ApplicationTitle", CultureInfo.InvariantCulture);
            var appVersion = App.VersionString;

            var formatted = string.Format(CultureInfo.InvariantCulture, $"{appName} {appVersion}");
            return formatted;
        }

        /// <summary>
        /// Activate the settings ViewModel
        /// </summary>
        public void ActivateSettingsClick()
        {
            var settingsVm = _settingsVmFactory.CreateSettingsViewModel();
            ActivateItem(settingsVm);
        }

        /// <summary>
        /// Activate the Main View
        /// </summary>
        public void ActivateMainClick()
        {
            var mainGridVm = _mainGridVmFactory.CreateMainGridViewModel();
            ActivateItem(mainGridVm);
        }

        /// <summary>
        /// Active the Debug View
        /// </summary>
        public void DebugClick()
        {
            var debugVm = _debugVmFactory.CreateDebugViewModel();
            ActivateItem(debugVm);
        }

    }
}
