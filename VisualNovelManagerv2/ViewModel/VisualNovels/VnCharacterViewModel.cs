using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.Pages;
using GalaSoft.MvvmLight.Command;
using System.Data.SQLite;
using System.IO;
using System.Net;
using VisualNovelManagerv2.CustomClasses;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnCharacterViewModel: ViewModelBase
    {

        public ICommand LoadCharacterCommand
        {
            get { return new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCharacterUrlList); }            
        }

        #region ObservableCollections
        static ObservableCollection<string> _vnCharacterCollection = new ObservableCollection<string>();
        public ObservableCollection<string> VnCharacterCollection
        {
            get { return _vnCharacterCollection; }
            set
            {
                _vnCharacterCollection = value;
                RaisePropertyChanged(nameof(VnCharacterCollection));
            }
        }
        

        #endregion

        public VnCharacterViewModel()
        {

        }

        #region Properties
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

        private int _selectedItemIndex;
        public int SelectedItemIndex
        {
            get { return _selectedItemIndex; }
            set
            {
                _selectedItemIndex = value;
                RaisePropertyChanged(nameof(SelectedItemIndex));
            }
        }


        #endregion



        private void LoadCharacterUrlList()
        {
            List<string> characterUrlList = new List<string>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ImageLink FROM VnCharacter WHERE VnId = @VnId ";
                        cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            characterUrlList.Add(reader["ImageLink"].ToString());
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

            DownloadCharacters(characterUrlList);
        }

        private void DownloadCharacters(List<string> characterList)
        {
            foreach (string character in characterList)
            {
                if(characterList.Count <1)return;
                if (!Directory.Exists(string.Format(@"{0}\Data\images\characters\{1}", Globals.DirectoryPath, Globals.VnId)))
                {
                    Directory.CreateDirectory(string.Format(@"{0}\Data\images\characters\{1}", Globals.DirectoryPath, Globals.VnId));
                }
                string filename = Path.GetFileNameWithoutExtension(character);
                string path = string.Format(@"{0}\Data\images\characters\{1}\{2}", Globals.DirectoryPath, Globals.VnId, Path.GetFileName(character));

                try
                {
                    if (!File.Exists(path))
                    {
                        WebClient client = new WebClient();
                        client.DownloadFile(new Uri(character),path );
                    }
                }
                catch (System.Net.WebException ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                }

            }
            LoadCharacterNameList();
        }

        private void LoadCharacterNameList()
        {
            VnCharacterCollection.Clear();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Name FROM VnCharacter WHERE VnId = @VnId ";
                        cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            VnCharacterCollection.Add(reader["Name"].ToString());
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
        }

        private void LoadCharacterData()
        {
            
        }

        private void sample()
        {
            VnCharacterModel.Name = "changed";
        }
    }

}
