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
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Title FROM VnRelease WHERE VnId = @VnId ";
                        cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            _releaseNameCollection.Add(title);
                        }
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SQLite.SQLiteException ex)
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

        private void LoadReleaseData()
        {
            DataSet dataSet = new DataSet();
            try
            {
                if (SelectedReleaseIndex < 0) return;
                _releaseLanguages.Clear();
                
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM VnRelease WHERE VnId=@VnId", connection);
                    cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dataSet);
                    connection.Close();
                }
                if (dataSet.Tables[0].Rows.Count <= 0) return;
                object[] releaseData = dataSet.Tables[0].Rows[SelectedReleaseIndex].ItemArray;

                VnReleaseModel.Title = releaseData[3].ToString();
                VnReleaseModel.OriginalTitle = releaseData[4].ToString();
                VnReleaseModel.Released = releaseData[5].ToString();
                VnReleaseModel.ReleaseType = releaseData[6].ToString();
                VnReleaseModel.Patch = releaseData[7].ToString();
                VnReleaseModel.Freeware = releaseData[8].ToString();
                VnReleaseModel.Doujin = releaseData[9].ToString();

                IEnumerable<string> languages = GetLangauges(releaseData[10].ToString());
                foreach (string language in languages)
                {
                    _releaseLanguages.Add(new ReleaseLanguagesCollection { VnReleaseModel = new VnReleaseModel { Languages = new BitmapImage(new Uri(language)) } });
                }

                VnReleaseModel.Website = releaseData[11].ToString();
                VnReleaseModel.Notes = ConvertRichTextDocument.ConvertToFlowDocument(releaseData[12].ToString());
                VnReleaseModel.MinAge = Convert.ToInt32(releaseData[13]);
                if (releaseData[14] != DBNull.Value)
                {
                    VnReleaseModel.Gtin = Convert.ToUInt64(releaseData[14]);
                }
                VnReleaseModel.Catalog = releaseData[15].ToString();
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
            
            LoadReleaseProducerData(Convert.ToInt32(dataSet.Tables[0].Rows[SelectedReleaseIndex].ItemArray[2]));            
        }

        private void LoadReleaseProducerData(int releaseId)
        {
            try
            {
                DataSet dataSet = new DataSet();
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM VnReleaseProducers WHERE ReleaseId=@ReleaseId",
                        connection);
                    cmd.Parameters.AddWithValue("@ReleaseId", releaseId);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dataSet);
                    connection.Close();
                }
                if (dataSet.Tables[0].Rows.Count <= 0) return;
                object[] releaseProducerData = dataSet.Tables[0].Rows[0].ItemArray;
                VnReleaseProducerModel.IsDeveloper = releaseProducerData[3].ToString();
                VnReleaseProducerModel.IsPublisher = releaseProducerData[4].ToString();
                VnReleaseProducerModel.Name = releaseProducerData[5].ToString();
                VnReleaseProducerModel.OriginalName = releaseProducerData[6].ToString();

                string type = releaseProducerData[7].ToString();
                switch (type)
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
                        VnReleaseProducerModel.Type = type;
                        break;
                }
            }
            catch (SQLiteException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
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
