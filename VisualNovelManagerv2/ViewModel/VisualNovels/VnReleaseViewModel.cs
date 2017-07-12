using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EntityFramework;
using VisualNovelManagerv2.EntityFramework.Entity.VnInfo;
using VisualNovelManagerv2.EntityFramework.Entity.VnOther;
using VisualNovelManagerv2.EntityFramework.Entity.VnRelease;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnReleaseViewModel: ViewModelBase
    {

        public ICommand LoadReleaseCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadReleaseNameList);
        

        public VnReleaseViewModel()
        {
            LoadReleaseNameList();
        }

        #region Properties
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

        #endregion

        private void LoadReleaseNameList()
        {
            ReleaseNameCollection.Clear();
            try
            {
                using (var db = new DatabaseContext("Database"))
                {
                    foreach (VnRelease release in db.Set<VnRelease>().Where(x=>x.VnId == Globals.VnId))
                    {
                        _releaseNameCollection.Add(release.Title);
                    }
                    db.Dispose();
                }
            }

            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private void LoadReleaseData()
        {
            int releaseId = 0;
            try
            {
                if (SelectedReleaseIndex < 0) return;
                _releaseLanguages.Clear();
               
                using (var db = new DatabaseContext("Database"))
                {
                    int index = (SelectedReleaseIndex + 1);
                    int count = 1;
                    foreach (VnRelease release in db.Set<VnRelease>().Where(x=>x.VnId==Globals.VnId))
                    {
                        if (count != index)
                        {
                            count++;
                        }
                        else
                        {
                            releaseId = Convert.ToInt32(release.ReleaseId);

                            VnReleaseModel.Title = release.Title;
                            VnReleaseModel.OriginalTitle = release.Original;
                            VnReleaseModel.Released = release.Released;
                            VnReleaseModel.ReleaseType = release.ReleaseType;
                            VnReleaseModel.Patch = release.Patch;
                            VnReleaseModel.Freeware = release.Freeware;
                            VnReleaseModel.Doujin = release.Doujin;
                            VnReleaseModel.Website = release.Website;
                            VnReleaseModel.Notes = ConvertRichTextDocument.ConvertToFlowDocument(release.Notes);
                            VnReleaseModel.MinAge = release.MinAge;
                            if (release.Gtin != null) VnReleaseModel.Gtin = Convert.ToUInt64(release.Gtin);
                            VnReleaseModel.Catalog = release.Catalog;
                            VnReleaseModel.Resolution = release.Resolution;
                            VnReleaseModel.Voiced = release.Voiced;
                            VnReleaseModel.Animation = release.Animation;
                            foreach (string language in GetLangauges(release.Languages))
                            {
                                if (language != null)
                                {
                                    _releaseLanguages.Add(new ReleaseLanguagesCollection
                                    {
                                        VnReleaseModel =
                                            new VnReleaseModel {Languages = new BitmapImage(new Uri(language))}
                                    });
                                }
                            }
                            break;
                        }
                    }
                    db.Dispose();
                }
            }
            catch (SQLiteException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            catch (System.IndexOutOfRangeException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
            LoadReleaseProducerData(releaseId);            
        }

        private void LoadReleaseProducerData(int releaseId)
        {
            try
            {
                using (var db = new DatabaseContext("Database"))
                {
                    foreach (VnReleaseProducers release in db.Set<VnReleaseProducers>().Where(x=>x.ReleaseId==releaseId))
                    {
                        VnReleaseProducerModel.IsDeveloper = release.Developer;
                        VnReleaseProducerModel.IsPublisher = release.Publisher;
                        VnReleaseProducerModel.Name = release.Name;
                        VnReleaseProducerModel.OriginalName = release.Original;

                        switch (release.ProducerType)
                        {
                            case "co":
                                VnReleaseProducerModel.Type = "Company";
                                break;
                            case "in":
                                VnReleaseProducerModel.Type = "Individual";
                                break;
                            case "ng":
                                VnReleaseProducerModel.Type = "Amateur group";
                                break;
                            default:
                                VnReleaseProducerModel.Type = release.ProducerType;
                                break;
                        }
                    }
                    db.Dispose();
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }

        private static IEnumerable<string> GetLangauges(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\Unknown.png")
                .ToList();
        }
    }

    public class ReleaseLanguagesCollection
    {
        public VnReleaseModel VnReleaseModel { get; set; }
    }
}
