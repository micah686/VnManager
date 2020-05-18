using MvvmDialogs;
using Stylet;
using StyletIoC;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels
{
    public class RootViewModel: Conductor<Screen>
    {

        public StatusBarViewModel StatusBarPage { get; set; }

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        private int _windowButtonPressedCounter = 0;

        public static RootViewModel Instance { get; private set; }


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
                    var brush = new SolidColorBrush((System.Windows.Media.Color)Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor));
                    SettingsIconColor = brush != null ? brush : System.Windows.Media.Brushes.LightSteelBlue;
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

            StatusBarPage = _container.Get<StatusBarViewModel>();


            var maingrid = _container.Get<MainGridViewModel>();
            ActivateItem(maingrid);
            var brush = new SolidColorBrush((System.Windows.Media.Color)Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor));
            SettingsIconColor = brush != null ? brush : System.Windows.Media.Brushes.LightSteelBlue;
            
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
