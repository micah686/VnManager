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
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VndbSharp.Models.Common;
// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnCharacterViewModel: ViewModelBase
    {

        public ICommand LoadCharacterCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCharacterUrlList);

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


        #endregion

        public VnCharacterViewModel()
        {            
            LoadCharacterNameList();
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
            SetMaxWidth();
            DownloadCharacters(characterUrlList);
        }

        private void DownloadCharacters(List<string> characterList)
        {
            try
            {
                foreach (string character in characterList)
                {
                    if (characterList.Count < 1) return;
                    if (!Directory.Exists($@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}"))
                    {
                        Directory.CreateDirectory($@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}");
                    }
                    string path = $@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}\{Path.GetFileName(character)}";

                    if (!File.Exists(path))
                    {
                        WebClient client = new WebClient();
                        client.DownloadFile(new Uri(character), path);
                    }
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
                            _characterNameCollection.Add(name);
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
            try
            {
                TraitsCollection.Clear();
                if (SelectedTraitIndex < 0 && _traitDescription.Blocks.Count >= 1)
                {
                    TraitDescription.Blocks.Clear();
                }
                //TraitDescription.Blocks.Clear();
                DataSet dataSet = new DataSet();
                int characterId;
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT CharacterId FROM VnCharacter WHERE Name= @Name AND VnId=@VnId", connection);
                    cmd.Parameters.AddWithValue("@Name", SelectedCharacter);
                    cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                    characterId = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM VnCharacter WHERE Name= @Name AND VnId=@VnId", connection);
                    cmd.Parameters.AddWithValue("@Name", SelectedCharacter);
                    cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dataSet);
                    connection.Close();
                }
                if (dataSet.Tables[0].Rows.Count < 1) return;
                {
                    object[] characterInfo = dataSet.Tables[0].Rows[0].ItemArray;
                    VnCharacterModel.Name = characterInfo[3].ToString();
                    VnCharacterModel.OriginalName = characterInfo[4].ToString();
                    VnCharacterModel.Gender = GetGenderIcon(characterInfo[5].ToString());
                    VnCharacterModel.BloodType = characterInfo[6].ToString();
                    VnCharacterModel.Birthday = characterInfo[7].ToString();

                    if (!string.IsNullOrEmpty(characterInfo[8].ToString()))
                    {
                        VnCharacterModel.Aliases = characterInfo[8].ToString().Contains(",") ? characterInfo[8].ToString().Replace(",", ", ") : characterInfo[8].ToString();
                    }
                    VnCharacterModel.Description = ConvertRichTextDocument.ConvertToFlowDocument(characterInfo[9].ToString());
                    string path =
                        $@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}\{
                                Path.GetFileName(characterInfo[10].ToString())
                            }";
                    BitmapImage bImage = new BitmapImage(new Uri(path));
                    VnCharacterModel.Image = bImage;

                    VnCharacterModel.Bust = characterInfo[11].ToString();
                    VnCharacterModel.Waist = characterInfo[12].ToString();
                    VnCharacterModel.Hip = characterInfo[13].ToString();
                    VnCharacterModel.Height = characterInfo[14].ToString();
                    VnCharacterModel.Weight = characterInfo[15].ToString();


                    using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                    {
                        connection.Open();
                        SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM VnCharacterTraits WHERE CharacterId=@CharacterId", connection);
                        cmd.Parameters.AddWithValue("@CharacterId", characterId);
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            _traitsCollection.Add(reader["TraitName"].ToString());
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                DebugLogging.WriteDebugLog(exception);
                throw;
            }            
        }

        private void BindTraitDescription()
        {
            if (SelectedTraitIndex >= 0)
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                    {
                        connection.Open();
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT Description FROM VnTraitData WHERE Name= @Name";
                            cmd.Parameters.AddWithValue("@Name", SelectedTrait);

                            TraitDescription = ConvertRichTextDocument.ConvertToFlowDocument(cmd.ExecuteScalar().ToString());
                        }
                    }
                }
                catch (SQLiteException ex)
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
