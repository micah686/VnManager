// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using Stylet;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Sentry;
using VnManager.Helpers;
using VnManager.Initializers;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;
using Colors = AdonisUI.Colors;

namespace VnManager.ViewModels
{
    
    public class RootViewModel : Conductor<IScreen>, INavigationControllerDelegate
    {
        public static StatusBarViewModel StatusBarPage { get; set; }

        private readonly IWindowManager _windowManager;
        private readonly Func<MainGridViewModel> _mainGridVmFactory;
        private readonly Func<SettingsViewModel> _settingsVmFactory;
        private readonly Func<AboutViewModel> _aboutVmFactory;
        private readonly Func<ImportViewModel> _importVm;
        private readonly Func<SetEnterPasswordViewModel> _enterPassVm;

        private int _windowButtonPressedCounter = 0;

        public static string WindowTitle => FormatWindowTitle();

        #region SettingsPressed
        private bool _isSettingsPressed;
        public bool IsSettingsPressed
        {
            get => _isSettingsPressed;
            set
            {
                if (_windowButtonPressedCounter == 0 || _windowButtonPressedCounter > 0 && !value)
                {
                    SetAndNotify(ref _isSettingsPressed, value);
                    if (_isSettingsPressed)
                    {
                        _windowButtonPressedCounter += 1;
                        SettingsIconColor = Brushes.LimeGreen;
                        ActivateSettingsClick();
                    }
                    else
                    {
                        _windowButtonPressedCounter -= 1;
                        var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
                        SettingsIconColor = result == null ? Brushes.LightSteelBlue : new SolidColorBrush((Color)result);
                        ActivateMainClick();
                    }
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
                if (_windowButtonPressedCounter == 0 || _windowButtonPressedCounter > 0 && !value)
                {
                    SetAndNotify(ref _debugPressed, value);
                    if (_debugPressed)
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
        }

        #endregion


#if DEBUG
        private readonly Func<DebugViewModel> _debugVmFactory;
        /// <summary>
        /// Main Root Window that all elements are added to
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="mainGridFactory"></param>
        /// <param name="settingsFactory"></param>
        /// <param name="import"></param>
        /// <param name="enterPass"></param>
        /// <param name="about"></param>
        /// <param name="debugFactory"></param>
        public RootViewModel(IWindowManager windowManager, Func<MainGridViewModel> mainGridFactory, Func<SettingsViewModel> settingsFactory, 
            Func<ImportViewModel> import, Func<SetEnterPasswordViewModel> enterPass, Func<AboutViewModel> about, Func<DebugViewModel> debugFactory)
        {
            try
            {
                _windowManager = windowManager;
                _mainGridVmFactory = mainGridFactory;
                _settingsVmFactory = settingsFactory;
                _aboutVmFactory = about;

                _importVm = import;
                _enterPassVm = enterPass;
                _debugVmFactory = debugFactory;
            
                StatusBarPage = new StatusBarViewModel();
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to create RootViewModel");
                SentryHelper.SendException(e, null, SentryLevel.Error);
                throw;
            }
        }
#else
        /// <summary>
        /// Main Root Window that all elements are added to
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="mainGridFactory"></param>
        /// <param name="settingsFactory"></param>
        /// <param name="import"></param>
        /// <param name="enterPass"></param>
        /// <param name="about"></param>
        public RootViewModel(IWindowManager windowManager, Func<MainGridViewModel> mainGridFactory, Func<SettingsViewModel> settingsFactory,
            Func<ImportViewModel> import, Func<SetEnterPasswordViewModel> enterPass, Func<AboutViewModel> about)
        {
            try
            {
                _windowManager = windowManager;
                _mainGridVmFactory = mainGridFactory;
                _settingsVmFactory = settingsFactory;
                _aboutVmFactory = about;

                _importVm = import;
                _enterPassVm = enterPass;

                StatusBarPage = new StatusBarViewModel();
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to create RootViewModel");
                SentrySdk.CaptureException(e);
                throw;
            }
        }
#endif


        /// <summary>
        /// Does checks when the actual app is "Drawn"
        /// </summary>
        protected override void OnViewLoaded()
        {
            try
            {
                _ = new Startup(_windowManager, _importVm, _enterPassVm);
                var mainGridVm = _mainGridVmFactory();
                ActivateItem(mainGridVm);
                var result = Application.Current.TryFindResource(Colors.ForegroundColor);
                SettingsIconColor = result == null ? Brushes.LightSteelBlue : new SolidColorBrush((Color)result);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed OnViewLoaded on RootViewModel");
                SentryHelper.SendException(e, null, SentryLevel.Error);
                throw;
            }
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
        /// Activates Settings View. Linked to title buttons
        /// <see cref="ActivateSettingsClick"/>
        /// </summary>
        public void ActivateSettingsClick()
        {
            try
            {
                ActivateItem(_settingsVmFactory());
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to activate Settings View");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                throw;
            }
        }
        /// <summary>
        /// Activates About Window. Linked to title buttons
        /// <see cref="ActivateAboutClick"/>
        /// </summary>
        public void ActivateAboutClick()
        {
            try
            {
                _windowManager.ShowDialog(_aboutVmFactory());
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to activate About View");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                throw;
            }
        }

        /// <summary>
        /// Activates Main Window.
        /// <see cref="ActivateMainClick"/>
        /// </summary>
        public void ActivateMainClick()
        {
            try
            {
                ActivateItem(_mainGridVmFactory());
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to activate Main View");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                throw;
            }
        }


        public void DebugClick()
        {
#if DEBUG
            ActivateItem(_debugVmFactory());
#else
            //Do Nothing
#endif
        }

        /// <summary>
        /// Navigates to a given Screen
        /// </summary>
        /// <param name="screen">Screen to navigate to</param>
        public void NavigateTo(IScreen screen)
        {
            try
            {
                this.ActivateItem(screen);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to Navigate To specified View");
                SentryHelper.SendException(e, null, SentryLevel.Error);
                throw;
            }
        }
    }
}
