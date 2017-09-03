using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualNovelManagerv2.Design.VisualNovel;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnRelease
{
    public partial class VnReleaseViewModel
    {
        private static ObservableCollection<string> _releaseNameCollection = new ObservableCollection<string>();
        public ObservableCollection<string> ReleaseNameCollection
        {
            get { return _releaseNameCollection; }
            set
            {
                _releaseNameCollection = value;
                RaisePropertyChanged(nameof(ReleaseNameCollection));
            }
        }

        private VnReleaseModel _vnReleaseModel = new VnReleaseModel();
        public VnReleaseModel VnReleaseModel
        {
            get { return _vnReleaseModel; }
            set
            {
                _vnReleaseModel = value;
                RaisePropertyChanged(nameof(VnReleaseModel));
            }
        }

        private VnReleaseProducerModel _vnReleaseProducerModel = new VnReleaseProducerModel();
        public VnReleaseProducerModel VnReleaseProducerModel
        {
            get { return _vnReleaseProducerModel; }
            set
            {
                _vnReleaseProducerModel = value;
                RaisePropertyChanged(nameof(VnReleaseProducerModel));
            }
        }


        private int _selectedReleaseIndex;
        public int SelectedReleaseIndex
        {
            get { return _selectedReleaseIndex; }
            set
            {
                _selectedReleaseIndex = value;
                RaisePropertyChanged(nameof(SelectedReleaseIndex));
            }
        }

        private string _selectedRelease;
        public string SelectedRelease
        {
            get { return _selectedRelease; }
            set
            {
                _selectedRelease = value;
                RaisePropertyChanged(nameof(SelectedRelease));
                LoadReleaseData();
            }
        }

        private ObservableCollection<ReleaseLanguagesCollection> _releaseLanguages = new ObservableCollection<ReleaseLanguagesCollection>();
        public ObservableCollection<ReleaseLanguagesCollection> ReleaseLanguages
        {
            get { return _releaseLanguages; }
            set
            {
                _releaseLanguages = value;
                RaisePropertyChanged(nameof(ReleaseLanguages));
            }
        }

        private ObservableCollection<ReleasePlatformsCollection> _releasePlatforms = new ObservableCollection<ReleasePlatformsCollection>();
        public ObservableCollection<ReleasePlatformsCollection> ReleasePlatforms
        {
            get { return _releasePlatforms; }
            set
            {
                _releasePlatforms = value;
                RaisePropertyChanged(nameof(ReleasePlatforms));
            }
        }
    }
}
