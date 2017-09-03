using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using VisualNovelManagerv2.Design.VisualNovel;

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

        private static ObservableCollection<string> _traitsCollection = new ObservableCollection<string>();
        public ObservableCollection<string> TraitsCollection
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

        private FlowDocument _traitDescription;
        public FlowDocument TraitDescription
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

        public ICommand LoadCharacterCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCharacterNameList);
    }
}
