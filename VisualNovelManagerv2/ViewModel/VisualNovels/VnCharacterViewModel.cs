using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VndbSharp.Models.Common;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnCharacterViewModel: ViewModelBase
    {

        public ICommand LoadCharacterCommand
        {
            get { return new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCharacterUrlList); }            
        }

        #region ObservableCollections

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


        #endregion

        public VnCharacterViewModel()
        {
            _vnCharacterModel = new VnCharacterModel();
            
            LoadCharacterUrlList();
        }

        #region Properties

        private static VnCharacterModel _vnCharacterModel;
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
            CharacterNameCollection.Clear();
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
                            string name = reader["Name"].ToString();
                            BitmapImage bitmap = new BitmapImage();
                            CharacterNameCollection.Add(name);
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
            SetMaxWidth();
        }

        private void SetMaxWidth()
        {
            if (CharacterNameCollection.Count > 0)
            {
                string longestString = CharacterNameCollection.OrderByDescending(s => s.Length).First();
                MaxWidth = MeasureStringSize.GetMaxStringWidth(longestString);
            }
        }

        private void LoadCharacterData()
        {
            
            DataSet dataSet = new DataSet();
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM VnCharacter WHERE Name= @Name AND VnId=@VnId", connection);
                cmd.Parameters.AddWithValue("@Name", SelectedCharacter);
                cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                adapter.Fill(dataSet);
                connection.Close();
            }

            var characterInfo = dataSet.Tables[0].Rows[0].ItemArray;
            VnCharacterModel.Name = characterInfo[3].ToString();
            VnCharacterModel.OriginalName = characterInfo[4].ToString();
            VnCharacterModel.Gender = GetGenderIcon(characterInfo[5].ToString());
            VnCharacterModel.BloodType = characterInfo[6].ToString();
            VnCharacterModel.Birthday = characterInfo[7].ToString();

            if (!string.IsNullOrEmpty(characterInfo[8].ToString()))
            {
                if (characterInfo[8].ToString().Contains(","))
                {
                    VnCharacterModel.Aliases = characterInfo[8].ToString().Replace(",", ", ");
                }
                else
                {
                    VnCharacterModel.Aliases = characterInfo[8].ToString();
                }
            }            
            VnCharacterModel.Description = ConvertRichTextDocument.ConvertToFlowDocument(characterInfo[9].ToString());
            string path = string.Format(@"{0}\Data\images\characters\{1}\{2}", Globals.DirectoryPath, Globals.VnId, Path.GetFileName(characterInfo[10].ToString()));
            BitmapImage bImage = new BitmapImage(new Uri(path));
            VnCharacterModel.Image = bImage;

            VnCharacterModel.Bust = characterInfo[11].ToString();
            VnCharacterModel.Waist = characterInfo[12].ToString();
            VnCharacterModel.Hip = characterInfo[13].ToString();
            VnCharacterModel.Height = characterInfo[14].ToString();
            VnCharacterModel.Weight = characterInfo[15].ToString();

        }

        private BitmapImage GetGenderIcon(string gender)
        {
            switch (gender)
            {
                case "Female":
                    return new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\gender\female.png"));
                case "Male":
                    return new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\gender\male.png"));
                case "Both":
                    return new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\gender\both.png"));
                default:
                    return null;
            }            
        }


    }
}
