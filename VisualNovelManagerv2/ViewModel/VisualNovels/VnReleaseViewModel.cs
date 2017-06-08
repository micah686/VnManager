using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
            if (SelectedReleaseIndex < 0) return;
            DataSet dataSet = new DataSet();
            int releaseId;
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM VnRelease WHERE VnId=@VnId", connection);
                cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                adapter.Fill(dataSet);
                connection.Close();
            }
            releaseId = Convert.ToInt32(dataSet.Tables[0].Rows[SelectedReleaseIndex].ItemArray[2]);
            object[] releaseData = dataSet.Tables[0].Rows[SelectedReleaseIndex].ItemArray;
            VnReleaseModel.Title = releaseData[3].ToString();
            VnReleaseModel.OriginalTitle = releaseData[4].ToString();
            VnReleaseModel.Released = releaseData[5].ToString();
            VnReleaseModel.ReleaseType = releaseData[6].ToString();
            VnReleaseModel.Patch = releaseData[7].ToString();
            VnReleaseModel.Freeware = releaseData[8].ToString();
            VnReleaseModel.Doujin = releaseData[9].ToString();
            
            VnReleaseModel.Website = releaseData[11].ToString();
            VnReleaseModel.Notes = ConvertRichTextDocument.ConvertToFlowDocument(releaseData[12].ToString());
            VnReleaseModel.MinAge = Convert.ToInt32(releaseData[13]);
            if (releaseData[14] != DBNull.Value)
            {
                VnReleaseModel.Gtin = Convert.ToUInt64(releaseData[14]);
            }
            
            VnReleaseModel.Catalog = releaseData[15].ToString();
        }
    }
}
