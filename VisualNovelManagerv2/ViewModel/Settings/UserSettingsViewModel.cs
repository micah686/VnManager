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
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.Design.Settings;

namespace VisualNovelManagerv2.ViewModel.Settings
{
    public class UserSettingsViewModel: ViewModelBase
    {
        public ICommand SaveSettingsCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(SaveSettings);

        #region Properties

        #region SelectedSpoilerLevel
        private SpoilerLevel _selectedSpoilerLevel = SpoilerLevel.None;
        public SpoilerLevel SelectedSpoilerLevel
        {
            get { return _selectedSpoilerLevel; }
            set
            {
                _selectedSpoilerLevel = value;
                switch (value)
                {
                    case SpoilerLevel.None:
                        ChosenSpoilerLevel = 0;
                        break;
                    case SpoilerLevel.Minor:
                        ChosenSpoilerLevel = 1;
                        break;
                    case SpoilerLevel.Major:
                        ChosenSpoilerLevel = 2;
                        break;
                    default:
                        ChosenSpoilerLevel = 0;
                        break;
                }
                RaisePropertyChanged(nameof(SelectedSpoilerLevel));
            }
        }
        #endregion

        #region SpoilerLevelList
        private List<KeyValuePair<string, SpoilerLevel>> _spoilerLevelList;
        public List<KeyValuePair<string, SpoilerLevel>> SpoilerLevelList
        {
            get
            {
                if (_spoilerLevelList == null)
                {
                    _spoilerLevelList = new List<KeyValuePair<string, SpoilerLevel>>();
                    foreach (SpoilerLevel level in Enum.GetValues(typeof(SpoilerLevel)))
                    {
                        FieldInfo fieldInfo = level.GetType().GetField(level.ToString());
                        DescriptionAttribute[] attributes =
                            (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        string description = attributes.Length > 0 ? attributes[0].Description : string.Empty;
                        KeyValuePair<string, SpoilerLevel> typeKeyValue =
                            new KeyValuePair<string, SpoilerLevel>(description, level);
                        _spoilerLevelList.Add(typeKeyValue);
                    }
                    return _spoilerLevelList;
                }
                return _spoilerLevelList;
            }
            set
            {
                _spoilerLevelList = value;
                RaisePropertyChanged(nameof(SpoilerLevelList));
            }
        }
        #endregion

        #region SelectedNsfwEnabled
        private NsfwEnabled _selectedNsfwEnabled;
        public NsfwEnabled SelectedNsfwEnabled
        {
            get { return _selectedNsfwEnabled; }
            set
            {
                _selectedNsfwEnabled = value;
                switch (value)
                {
                    case NsfwEnabled.Yes:
                        IsNsfwEnabled = true;
                        break;
                    case NsfwEnabled.No:
                        IsNsfwEnabled = false;
                        break;
                    default:
                        IsNsfwEnabled = false;
                        break;
                }
                RaisePropertyChanged(nameof(SelectedNsfwEnabled));
            }
        }
        #endregion

        #region NsfwEnabledList
        private List<KeyValuePair<string, NsfwEnabled>> _nsfwEnabledList;
        public List<KeyValuePair<string, NsfwEnabled>> NsfwEnabledList
        {
            get
            {
                if (_nsfwEnabledList == null)
                {
                    _nsfwEnabledList = new List<KeyValuePair<string, NsfwEnabled>>();
                    foreach (NsfwEnabled level in Enum.GetValues(typeof(NsfwEnabled)))
                    {
                        FieldInfo fieldInfo = level.GetType().GetField(level.ToString());
                        DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        string description = attributes.Length > 0 ? attributes[0].Description : string.Empty;
                        KeyValuePair<string, NsfwEnabled> typeKeyValue = new KeyValuePair<string, NsfwEnabled>(description, level);
                        _nsfwEnabledList.Add(typeKeyValue);
                    }
                    return _nsfwEnabledList;
                }
                return _nsfwEnabledList;
            }
            set
            {
                _nsfwEnabledList = value;
                RaisePropertyChanged(nameof(NsfwEnabledList));
            }
        }
        #endregion

        public bool IsNsfwEnabled { get; set; }
        public int ChosenSpoilerLevel { get; set; }
        #endregion

        public UserSettingsViewModel()
        {
            
        }
        
        private void SaveSettings()
        {
            Globals.NsfwEnabled = IsNsfwEnabled;
            Globals.MaxSpoiler = ChosenSpoilerLevel;

            UserSettings userSettings = new UserSettings
            {
                NsfwEnabled = IsNsfwEnabled,
                MaxSpoilerLevel = ChosenSpoilerLevel
            };
            ModifyUserSettings.SaveUserSettings(userSettings);
        }

    }


    public enum SpoilerLevel
    {
        [Description("None")]
        None = 0,
        [Description("Minor")]
        Minor = 1,
        [Description("Major")]
        Major = 2
    }

    public enum NsfwEnabled
    {
        [Description("No")]
        No = 0,
        [Description("Yes")]
        Yes = 1
    }

}
