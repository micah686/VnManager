using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnCharacter
{
    public partial class VnCharacterViewModel
    {

        private static ObservableCollection<string> _characterNameCollection = new ObservableCollection<string>();
        public ObservableCollection<string> CharacterNameCollection
        {
            get { return _characterNameCollection; }
            set
            {
                _characterNameCollection = value;
                RaisePropertyChanged(nameof(CharacterNameCollection));
            }
        }

        private static RangeEnabledObservableCollection<string> _traitsCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> TraitsCollection
        {
            get { return _traitsCollection; }
            set
            {
                _traitsCollection = value;
                RaisePropertyChanged(nameof(TraitsCollection));
            }
        }

        private static VnCharacterModel _vnCharacterModel = new VnCharacterModel();
        public VnCharacterModel VnCharacterModel
        {
            get { return _vnCharacterModel; }
            set
            {
                _vnCharacterModel = value;
                RaisePropertyChanged(nameof(VnCharacterModel));
            }
        }

        private double _maxWidth;
        public double MaxWidth
        {
            get { return _maxWidth; }
            set
            {
                _maxWidth = value;
                RaisePropertyChanged(nameof(MaxWidth));
            }
        }

        private int _selectedCharacterIndex;
        public int SelectedCharacterIndex
        {
            get { return _selectedCharacterIndex; }
            set
            {
                _selectedCharacterIndex = value;
                RaisePropertyChanged(nameof(SelectedCharacterIndex));
            }
        }

        private string _selectedCharacter;
        public string SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                _selectedCharacter = value;
                RaisePropertyChanged(nameof(SelectedCharacter));
                LoadCharacterData();
            }
        }


        private int _selectedTraitIndex;
        public int SelectedTraitIndex
        {
            get { return _selectedTraitIndex; }
            set
            {
                _selectedTraitIndex = value;
                RaisePropertyChanged(nameof(SelectedTraitIndex));
            }
        }

        private string _selectedTrait;
        public string SelectedTrait
        {
            get { return _selectedTrait; }
            set
            {
                _selectedTrait = value;
                BindTraitDescription();
                RaisePropertyChanged(nameof(SelectedTrait));

            }
        }

        private string _traitDescription;
        public string TraitDescription
        {
            get { return _traitDescription; }
            set
            {
                _traitDescription = value;
                RaisePropertyChanged(nameof(TraitDescription));
            }
        }


        private string _genderColor;
        public string GenderColor
        {
            get { return _genderColor; }
            set
            {
                _genderColor = value;
                RaisePropertyChanged(nameof(GenderColor));
            }
        }

        private VnTraitData[] _traitArray;
        public bool IsCharacterDownloading = false;

        public ICommand LoadCharacterCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCharacterNameList);
        public ICommand ClearCharacterDataCommand => new RelayCommand(ClearCharacterData);
    }
}
