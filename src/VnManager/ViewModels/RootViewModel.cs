using MvvmDialogs;
using Stylet;
using StyletIoC;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using AdysTech.CredentialManager;
using LiteDB;
using VnManager.Helpers;
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


        public RootViewModel(IContainer container, IWindowManager windowManager)
        {
            Instance = this;
            _container = container;
            _windowManager = windowManager;
            App.StatusBar = _container.Get<StatusBarViewModel>();



        }

        protected override void OnViewLoaded()
        {

            if (!IsNormalStart())
            {
                ////remove any previous credentials
                //string[] credStrings = new[] { App.CredDb, App.CredFile };
                //foreach (var cred in credStrings)
                //{
                //    var value = CredentialManager.GetCredentials(cred);
                //    if (value != null)
                //    {
                //        CredentialManager.RemoveCredentials(cred);
                //    }
                //}
                App.UserSettings = UserSettingsHelper.ReadUserSettings();
                var auth = _container.Get<SetEnterPasswordViewModel>();
                var isAuth = _windowManager.ShowDialog(auth);
                
                if (isAuth == true)
                {
                    App.UserSettings = UserSettingsHelper.ReadUserSettings();//read for any changed user settings
                    CheckForImportDb();
                    var mainGrid = _container.Get<MainGridViewModel>();
                    ActivateItem(mainGrid);
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
                CheckDbError();
                var mainGrid = _container.Get<MainGridViewModel>();
                ActivateItem(mainGrid);
                StatusBarPage = _container.Get<StatusBarViewModel>();
                var result = Application.Current.TryFindResource(AdonisUI.Colors.ForegroundColor);
                SettingsIconColor = result == null ? System.Windows.Media.Brushes.LightSteelBlue : new SolidColorBrush((System.Windows.Media.Color)result);
            }




        }

        /// <summary>
        /// Checks to see if the user has been asked to import the database yet. If not, prompts for importing data into the Db
        /// </summary>
        private void CheckForImportDb()
        {
            if (App.UserSettings.DidAskImportDb != false)
            {
                return;
            }
            var result = _windowManager.ShowMessageBox(App.ResMan.GetString("AskImportDb"),
                App.ResMan.GetString("ImportDataTitle"), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var vm = _container.Get<ImportViewModel>();
                _windowManager.ShowDialog(vm);
            }
            

            App.UserSettings.DidAskImportDb = true;
            UserSettingsHelper.SaveUserSettings(App.UserSettings);
        }


        //should exit if it can't read the database
        /// <summary>
        /// Checks if the program can successfully open the database.
        /// If it encounters an error, the program will exit, as the program can't function if it can't read the database
        /// </summary>
        private void CheckDbError()
        {
            string appExit = App.ResMan.GetString("AppExit");
            string dbError = App.ResMan.GetString("DbError");

            string errorStr;
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    errorStr = $"{App.ResMan.GetString("PasswordNoEmpty")}\n{appExit}";
                    _windowManager.ShowMessageBox(errorStr, dbError);
                    Environment.Exit(1);
                }
                else
                {
                    using (var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}"))
                    {
                        //do nothing. This is checking if the database can be opened
                    }
                }
            }
            catch (IOException)
            {
                errorStr = $"{App.ResMan.GetString("DbIsLockedProc")}\n{appExit}";
                _windowManager.ShowMessageBox(errorStr, dbError);
                Environment.Exit(1);
            }
            catch (LiteException ex)
            {
                if (ex.Message == "Invalid password")
                {
                    errorStr = $"{App.ResMan.GetString("PassIncorrect")}\n{appExit}";
                    _windowManager.ShowMessageBox(errorStr, dbError);
                    Environment.Exit(1);
                }
                else
                {
                    errorStr = $"{ex.Message}\n{appExit}";
                    _windowManager.ShowMessageBox(errorStr, dbError);
                    Environment.Exit(1);
                }
            }
            catch (Exception)
            {
                errorStr = $"{App.ResMan.GetString("UnknownException")}\n{appExit}";
                _windowManager.ShowMessageBox(errorStr, dbError);
                Environment.Exit(1);
            }
        }


        private static bool IsNormalStart()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.json");
            if (!File.Exists(configFile))
            {
                return false;
            }
            if (!UserSettingsHelper.ValidateConfigFile())
            {
                return false;
            }
            if (CredentialManager.GetCredentials(App.CredDb) == null)
            {
                return false;
            }
            App.UserSettings = UserSettingsHelper.ReadUserSettings();
            var useEncryption = App.UserSettings.RequirePasswordEntry;
            return !useEncryption;
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
