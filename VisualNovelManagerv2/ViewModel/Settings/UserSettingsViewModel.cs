using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Design.Settings;
using VisualNovelManagerv2.ViewModel.VisualNovels;

namespace VisualNovelManagerv2.ViewModel.Settings
{
    public class UserSettingsViewModel: ViewModelBase
    {
        public ICommand SaveSettingsCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(SaveSettings);

        #region Properties

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

        public int SpoilerLevel { get; set; }
        #endregion

        public UserSettingsViewModel()
        {
            NsfwEnabledCollection.Add(false);
            NsfwEnabledCollection.Add(true);
            
            SpoilerLevelCollection.Add("None");
            SpoilerLevelCollection.Add("Minor");
            SpoilerLevelCollection.Add("Major");
        }
        
        private void SaveSettings()
        {
            Globals.NsfwEnabled = SelectedNsfwEnabled;
            Globals.MaxSpoiler = SpoilerLevel;

            UserSettings userSettings = new UserSettings
            {
                NsfwEnabled = SelectedNsfwEnabled,
                MaxSpoilerLevel = SpoilerLevel
            };
            ModifyUserSettings.SaveUserSettings(userSettings);

            var ssvm = ServiceLocator.Current.GetInstance<VnScreenshotViewModel>();
            ssvm.BindScreenshotsCommand.Execute(null);
        }

    }
}
