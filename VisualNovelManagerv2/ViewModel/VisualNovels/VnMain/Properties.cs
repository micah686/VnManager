using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnOther;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMain
{
    public partial class VnMainViewModel
    {

        #region ObservableVnInfoAnime
        private ObservableCollection<VnInfoAnime> _vnAnimeCollection = new ObservableCollection<VnInfoAnime>();
        public ObservableCollection<VnInfoAnime> VnInfoAnimeCollection
        {
            get { return _vnAnimeCollection; }
            set
            {
                _vnAnimeCollection = value;
                RaisePropertyChanged(nameof(VnInfoAnimeCollection));
            }
        }
        #endregion

        #region ObservableLanguageCollection
        private RangeEnabledObservableCollection<LanguagesCollection> _languageCollection = new RangeEnabledObservableCollection<LanguagesCollection>();
        public RangeEnabledObservableCollection<LanguagesCollection> LanguageCollection
        {
            get { return _languageCollection; }
            set
            {
                _languageCollection = value;
                RaisePropertyChanged(nameof(LanguageCollection));
            }
        }
        #endregion

        #region ObserableOriginalLanguageCollection
        private ObservableCollection<OriginalLanguagesCollection> _originalLanguagesCollection = new ObservableCollection<OriginalLanguagesCollection>();
        public ObservableCollection<OriginalLanguagesCollection> OriginalLanguagesCollection
        {
            get { return _originalLanguagesCollection; }
            set
            {
                _originalLanguagesCollection = value;
                RaisePropertyChanged(nameof(OriginalLanguagesCollection));
            }
        }
        #endregion

        #region ObservablePlatformCollection
        private ObservableCollection<PlatformCollection> _platformCollection = new ObservableCollection<PlatformCollection>();
        public ObservableCollection<PlatformCollection> PlatformCollection
        {
            get { return _platformCollection; }
            set
            {
                _platformCollection = value;
                RaisePropertyChanged(nameof(PlatformCollection));
            }
        }
        #endregion

        #region VnInfoRelation
        private ObservableCollection<VnInfoRelation> _vnInfoRelation = new ObservableCollection<VnInfoRelation>();
        public ObservableCollection<VnInfoRelation> VnInfoRelation
        {
            get { return _vnInfoRelation; }
            set
            {
                _vnInfoRelation = value;
                RaisePropertyChanged(nameof(VnInfoRelation));
            }
        }
        #endregion

        #region ObservableCollectionVnTag
        private RangeEnabledObservableCollection<string> _vnInfoTagCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> VnInfoTagCollection
        {
            get { return _vnInfoTagCollection; }
            set
            {
                _vnInfoTagCollection = value;
                RaisePropertyChanged(nameof(VnInfoTagCollection));
            }
        }
        #endregion

        #region ObservableTreeVnCategory
        private ObservableCollection<MenuItem> _treeVnCategories= new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> TreeVnCategories
        {
            get { return _treeVnCategories; }
            set
            {
                _treeVnCategories = value;
                RaisePropertyChanged(nameof(TreeVnCategories));
            }
        }
        #endregion        

        #region VnMainModel
        private VnMainModel _vnMainModel = new VnMainModel();
        public VnMainModel VnMainModel
        {
            get { return _vnMainModel; }
            set
            {
                _vnMainModel = value;
                RaisePropertyChanged(nameof(VnMainModel));
            }
        }
        #endregion

        #region SelectedTag
        private string _selectedTag;
        public string SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                _selectedTag = value;
                RaisePropertyChanged(nameof(SelectedTag));
                BindTagDescription();
            }
        }
        #endregion

        #region SelectedTagIndex
        private int? _selectedTagIndex;
        public int? SelectedTagIndex
        {
            get { return _selectedTagIndex; }
            set
            {
                _selectedTagIndex = value;
                RaisePropertyChanged(nameof(SelectedTagIndex));
            }
        }
        #endregion

        #region TagDescription
        private string _tagDescription;
        public string TagDescription
        {
            get { return _tagDescription; }
            set
            {
                _tagDescription = value;
                RaisePropertyChanged(nameof(TagDescription));
            }
        }
        #endregion

        #region IsPlayEnabled
        private bool _isPlayEnabled = true;
        public bool IsPlayEnabled
        {
            get { return _isPlayEnabled; }
            set
            {
                _isPlayEnabled = value;
                RaisePropertyChanged(nameof(IsPlayEnabled));
            }
        }
        #endregion


        private readonly Stopwatch _stopwatch = new Stopwatch();
        private string _selectedVn= String.Empty;
        private string _selectedCategory = String.Empty;
        public bool IsMainBinding = false;
        private bool _isUserInputEnabled = true;
        private bool _isGameRunning = false;

        public ICommand StartVnCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(StartVn);
        public ICommand OpenContextMenuCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(CreateContextMenu);
        public ICommand AddRemoveCategoryCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadAddRemoveCategoryWindow);
        public ICommand AddToCategoryCommand { get; private set; }
        public ICommand RemoveFromCategoryCommand { get; private set; }
        public ICommand DeleteVnCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(DeleteVn);
        public ICommand GetVnDataCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<Object>(CheckMenuItemName);
    }
}
