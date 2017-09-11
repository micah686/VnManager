using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        #region ObservableVnNameCollection
        private RangeEnabledObservableCollection<string> _vnNameCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> VnNameCollection
        {
            get { return _vnNameCollection; }
            set
            {
                _vnNameCollection = value;
                RaisePropertyChanged(nameof(VnNameCollection));
            }
        }
        #endregion

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
        private ObservableCollection<LanguagesCollection> _languageCollection = new ObservableCollection<LanguagesCollection>();
        public ObservableCollection<LanguagesCollection> LanguageCollection
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

        #region ObservableCategories
        private RangeEnabledObservableCollection<string> _categoriesCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> CategoriesCollection
        {
            get
            {
                //TODO:move this to another method maybe?
                using (var db = new DatabaseContext())
                {
                    foreach (Category category in db.Set<Category>())
                    {
                        _categoriesCollection.Add(category.CategoryName);
                    }
                    db.Dispose();
                }
                return _categoriesCollection;
            }
            set
            {
                _categoriesCollection = value;
                RaisePropertyChanged(nameof(CategoriesCollection));
            }
        }

        #endregion ObservableCategories


        #region maxwidth
        private double _maxListWidth;
        public double MaxListWidth
        {
            get { return _maxListWidth; }
            set
            {
                _maxListWidth = value;
                RaisePropertyChanged(nameof(MaxListWidth));
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

        #region SelectedVn
        private string _selectedVn;
        public string SelectedVn
        {
            get { return _selectedVn; }
            set
            {
                _selectedVn = value;
                RaisePropertyChanged(nameof(SelectedVn));
                GetVnData();
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
        private FlowDocument _tagDescription;
        public FlowDocument TagDescription
        {
            get { return _tagDescription; }
            set
            {
                _tagDescription = value;
                RaisePropertyChanged(nameof(TagDescription));
            }
        }
        #endregion

        #region SelectedCategory
        private string _selectedCategory;
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                LoadCategories();
                RaisePropertyChanged(nameof(SelectedCategory));
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

        public static bool IsDownloading = false;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public ICommand StartVnCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(StartVn);
        public ICommand OpenContextMenuCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(CreateContextMenu);
        public ICommand AddRemoveCategoryCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadAddRemoveCategoryWindow);
        public ICommand AddToCategoryCommand { get; private set; }
        public ICommand RemoveFromCategoryCommand { get; private set; }
        public ICommand DeleteVnCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(DeleteVn);

    }
}
