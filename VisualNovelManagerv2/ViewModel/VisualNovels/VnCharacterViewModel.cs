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
using VisualNovelManagerv2.EntityFramework;
using VisualNovelManagerv2.EntityFramework.Entity.VnCharacter;
using VisualNovelManagerv2.EntityFramework.Entity.VnRelease;
using VisualNovelManagerv2.EntityFramework.Entity.VnTagTrait;
using VndbSharp.Models.Common;
// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnCharacterViewModel: ViewModelBase
    {

        public ICommand LoadCharacterCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCharacterNameList);

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


        private void LoadCharacterNameList()
        {
            CharacterNameCollection.Clear();
            try
            {
                using (var db = new DatabaseContext("Database"))
                {
                    foreach (VnCharacter character in db.Set<VnCharacter>().Where(x => x.VnId == Globals.VnId))
                    {
                        _characterNameCollection.Add(character.Name);
                    }
                    db.Dispose();
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            SetMaxWidth();
            LoadCharacterUrlList();
        }

        private void LoadCharacterUrlList()
        {
            List<string> characterUrlList = new List<string>();
            try
            {
                using (var db = new DatabaseContext("Database"))
                {
                    foreach (var character in db.Set<VnCharacter>().Where(x => x.VnId == Globals.VnId).Select(p => p.ImageLink))
                    {
                        characterUrlList.Add(character);
                    }
                    db.Dispose();
                }
            }
            catch (Exception ex)
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
            
            //LoadCharacterNameList();
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

                using (var db = new DatabaseContext("Database"))
                {
                    foreach (var character in db.Set<VnCharacter>().Where(n => n.Name == SelectedCharacter).Where(i => i.VnId == Globals.VnId))
                    {
                        VnCharacterModel.Name = character.Name;
                        VnCharacterModel.OriginalName = character.Original;
                        VnCharacterModel.Gender = GetGenderIcon(character.Gender);
                        VnCharacterModel.BloodType = character.BloodType;
                        VnCharacterModel.Birthday = character.Birthday;
                        VnCharacterModel.Description = ConvertRichTextDocument.ConvertToFlowDocument(character.Description);
                        if (string.IsNullOrEmpty(character.Aliases))
                            VnCharacterModel.Aliases = string.Empty;
                        else
                            VnCharacterModel.Aliases = character.Aliases.Contains(",")
                                ? character.Aliases.Replace(",", ", ")
                                : character.Aliases;

                        string path = $@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}\{Path.GetFileName(character.ImageLink)}";
                        VnCharacterModel.Image = new BitmapImage(new Uri(path));
                        VnCharacterModel.Bust = character.Bust.ToString();
                        VnCharacterModel.Waist = character.Waist.ToString();
                        VnCharacterModel.Hip = character.Hip.ToString();
                        VnCharacterModel.Height = character.Height.ToString();
                        VnCharacterModel.Weight = character.Weight.ToString();

                        foreach (VnCharacterTraits trait in db.Set<VnCharacterTraits>().Where(c=>c.CharacterId== character.CharacterId))
                        {
                            _traitsCollection.Add(trait.TraitName);
                        }
                        break;
                    }                    
                    db.Dispose();
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
                    using (var db = new DatabaseContext("Database"))
                    {
                        foreach (string trait in db.Set<VnTraitData>().Where(n => n.Name == SelectedTrait).Select(d => d.Description))
                        {
                            TraitDescription = ConvertRichTextDocument.ConvertToFlowDocument(trait);
                            break;
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
