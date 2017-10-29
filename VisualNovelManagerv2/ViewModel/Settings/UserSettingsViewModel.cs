using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Presentation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Model.Settings;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnCharacter;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMain;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnRelease;

namespace VisualNovelManagerv2.ViewModel.Settings
{
    public class UserSettingsViewModel: ViewModelBase
    {
        public ICommand SaveSettingsCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(SaveSettings);
        public ICommand ResetSettingsCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(ResetSettings);

        #region Properties

        #region SelectedSpoilerLevelIndex
        private byte _selectedSpoilerLevelIndex;
        public byte SelectedSpoilerLevelIndex
        {
            get { return _selectedSpoilerLevelIndex; }
            set
            {
                _selectedSpoilerLevelIndex = value;
                RaisePropertyChanged(nameof(SelectedSpoilerLevelIndex));
            }
        }
        #endregion

        #region SelectedSpoilerLevel
        private string _selectedSpoilerLevel;
        public string SelectedSpoilerLevel
        {
            get { return _selectedSpoilerLevel; }
            set
            {
                _selectedSpoilerLevel = value;
                switch (value)
                {
                    case "None":
                        SpoilerLevel = 0;
                        break;
                    case "Minor":
                        SpoilerLevel = 1;
                        break;
                    case "Major":
                        SpoilerLevel = 2;
                        break;
                    default:
                        SpoilerLevel = 0;
                        break;
                }
                RaisePropertyChanged(nameof(SelectedSpoilerLevel));
            }
        }
        #endregion

        #region SpoilerLevelCollection
        private ObservableCollection<string> _spoilerLevelCollection = new ObservableCollection<string>();
        public ObservableCollection<string> SpoilerLevelCollection
        {
            get { return _spoilerLevelCollection; }
            set
            {
                _spoilerLevelCollection = value;
                RaisePropertyChanged(nameof(SpoilerLevelCollection));
            }
        }
        #endregion

        #region SelectedNsfwIndex
        private byte _selectedNsfwIndex;
        public byte SelectedNsfwIndex
        {
            get { return _selectedNsfwIndex; }
            set
            {
                _selectedNsfwIndex = value;
                RaisePropertyChanged(nameof(SelectedNsfwIndex));
            }
        }
        #endregion

        #region SelectedNsfwEnabled
        private bool _selectedNsfwEnabled;
        public bool SelectedNsfwEnabled
        {
            get { return _selectedNsfwEnabled; }
            set
            {
                _selectedNsfwEnabled = value;
                RaisePropertyChanged(nameof(SelectedNsfwEnabled));
            }
        }
        #endregion

        #region NsfwEnabledCollection
        private ObservableCollection<bool> _nsfwEnabledCollection= new ObservableCollection<bool>();
        public ObservableCollection<bool> NsfwEnabledCollection
        {
            get { return _nsfwEnabledCollection; }
            set
            {
                _nsfwEnabledCollection = value;
                RaisePropertyChanged(nameof(NsfwEnabledCollection));
            }
        }

        #endregion

        public byte SpoilerLevel { get; set; }
        #endregion

        public UserSettingsViewModel()
        {
            NsfwEnabledCollection.Add(false);
            NsfwEnabledCollection.Add(true);
            
            SpoilerLevelCollection.Add("None");
            SpoilerLevelCollection.Add("Minor");
            SpoilerLevelCollection.Add("Major");

            SelectedSpoilerLevelIndex = Globals.MaxSpoiler;
            switch (Globals.NsfwEnabled)
            {
                case true:
                    SelectedNsfwIndex = 1;
                    break;
                case false:
                    SelectedNsfwIndex = 0;
                    break;
                default:
                    SelectedNsfwIndex = 0;
                    break;
            }
        }
        
        private void SaveSettings()
        {
            Globals.NsfwEnabled = SelectedNsfwEnabled;
            Globals.MaxSpoiler = SpoilerLevel;

            UserSettings userSettings = new UserSettings
            {
                NsfwEnabled = SelectedNsfwEnabled,
                MaxSpoilerLevel = SpoilerLevel,
                AccentColor = AppearanceManager.Current.AccentColor.ToString()
            };
            ModifyUserSettings.SaveUserSettings(userSettings);

            var cvm = ServiceLocator.Current.GetInstance<VnCharacterViewModel>();
            var ssvm = ServiceLocator.Current.GetInstance<VnScreenshotViewModel>();
            var rvm = ServiceLocator.Current.GetInstance<VnReleaseViewModel>();
            var mvm = ServiceLocator.Current.GetInstance<VnMainViewModel>();

            mvm.BindVnDataPublic();
            cvm.ClearCharacterDataCommand.Execute(null);
            cvm.LoadCharacterCommand.Execute(null);
            rvm.ClearReleaseDataCommand.Execute(null);
            rvm.LoadReleaseNamesCommand.Execute(null);
            ssvm.BindScreenshotsCommand.Execute(null);

            Messenger.Default.Send(new NotificationMessage("Saved Settings"));
        }

        private void ResetSettings()
        {
            var msg = new NotificationMessageAction<MessageBoxResult>(this, "Reset Settings", (r) =>
            {
                if (r == MessageBoxResult.Yes)
                {
                    UserSettings userSettings = new UserSettings();
                    File.Delete(Globals.DirectoryPath + @"/Data/config/config.xml");
                    userSettings.NsfwEnabled = false;
                    userSettings.MaxSpoilerLevel = 0;
                    ModifyUserSettings.SaveUserSettings(userSettings);

                    var cvm = ServiceLocator.Current.GetInstance<VnCharacterViewModel>();
                    var ssvm = ServiceLocator.Current.GetInstance<VnScreenshotViewModel>();
                    var rvm = ServiceLocator.Current.GetInstance<VnReleaseViewModel>();
                    var mvm = ServiceLocator.Current.GetInstance<VnMainViewModel>();

                    mvm.BindVnDataPublic();
                    cvm.ClearCharacterDataCommand.Execute(null);
                    cvm.LoadCharacterCommand.Execute(null);
                    rvm.ClearReleaseDataCommand.Execute(null);
                    rvm.LoadReleaseNamesCommand.Execute(null);
                    ssvm.BindScreenshotsCommand.Execute(null);

                    Messenger.Default.Send(new NotificationMessage("Reset Settings"));
                }
            });
            Messenger.Default.Send(msg);
        }

    }
}
