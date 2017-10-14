using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.Converters.TraitConverter.Models;
using VisualNovelManagerv2.Converters.TraitConverter.TraitService;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnCharacter;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMain;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnCharacter
{
    public partial class VnCharacterViewModel: ViewModelBase
    {
        private readonly ITraitService _TraitService = new TraitService();
        public VnCharacterViewModel()
        {
            LoadCharacterNameList();
        }
        
        private void LoadCharacterNameList()
        {
            CharacterNameCollection.Clear();
            try
            {
                using (var context = new DatabaseContext())
                {
                    var nonSpoiledCharacters = context.VnCharacterVns
                        .Where(x => x.SpoilerLevel <= Globals.MaxSpoiler && x.VnId == Globals.VnId)
                        .Select(x => x.CharacterId).ToArray();
                    foreach (EF.Entity.VnCharacter.VnCharacter character in context.VnCharacter.Where(x => nonSpoiledCharacters.Contains(x.CharacterId)))
                    {
                        _characterNameCollection.Add(character.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }
            SetMaxWidth();
        }

        private void ClearCharacterData()
        {
            VnCharacterModel.Name = String.Empty;
            VnCharacterModel.Aliases = String.Empty;
            VnCharacterModel.Birthday = String.Empty;
            VnCharacterModel.BloodType = String.Empty;
            VnCharacterModel.Bust = String.Empty;
            VnCharacterModel.Description = String.Empty;
            VnCharacterModel.Gender = String.Empty;
            VnCharacterModel.Height = String.Empty;
            VnCharacterModel.Hip = String.Empty;
            VnCharacterModel.Image= new BitmapImage();
            VnCharacterModel.OriginalName = String.Empty;
            VnCharacterModel.Waist = String.Empty;
            VnCharacterModel.Weight = String.Empty;
            TraitDescription = String.Empty;
        }

        public async Task DownloadCharacterImagesPublic()
        {
            await DownloadCharacters();
        }

        private async Task DownloadCharacters()
        {
            if (ConnectionTest.VndbTcpSocketTest() == false)
            {
                Globals.Logger.Warn("Could not connect to Vndb API over SSL");
                Globals.StatusBar.SetOnlineStatusColor(false);
                Globals.StatusBar.IsShowOnlineStatusEnabled = true;
                await Task.Delay(3500);
                Globals.StatusBar.IsShowOnlineStatusEnabled = false;
                return;
            }
            try
            {
                IsCharacterDownloading = true;
                Globals.StatusBar.IsWorkProcessing = true;
                Globals.StatusBar.ProgressText = "Downloading Characters";
                Globals.StatusBar.IsDownloading = true;
                using (var context = new DatabaseContext())
                {
                    List<string> characterList = context.VnCharacter.Where(x => x.VnId == Globals.VnId).Select(i => i.ImageLink).ToList();
                    if (characterList.Count <= 0) return;
                    foreach (string character in characterList)
                    {
                        if (!Directory.Exists($@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}"))
                        {
                            Directory.CreateDirectory($@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}");
                        }
                        string path = $@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}\{Path.GetFileName(character)}";

                        if (!File.Exists(path) && !string.IsNullOrEmpty(character))
                        {
                            WebClient client = new WebClient();
                            await client.DownloadFileTaskAsync(new Uri(character), path);
                            client.Dispose();
                        }
                    }
                }
                Globals.StatusBar.IsDownloading = false;
                Globals.StatusBar.IsWorkProcessing = false;
                Globals.StatusBar.ProgressText = string.Empty;
                IsCharacterDownloading = false;
            }
            catch (System.Net.WebException ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }
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
                TraitCollection.Clear();
                if (SelectedTraitIndex < 0 && !string.IsNullOrEmpty(TraitDescription))
                {
                    TraitDescription = String.Empty;
                }

                using (var context = new DatabaseContext())
                {
                    foreach (var character in context.VnCharacter.Where(x => x.Id == context.VnCharacter
                    .Where(v => v.VnId == Globals.VnId).ToArray()[SelectedCharacterIndex].Id).Where(i => i.VnId == Globals.VnId))
                    {
                        _characterId = character.CharacterId;
                        VnCharacterModel.Name = character.Name;
                        VnCharacterModel.OriginalName = character.Original;
                        VnCharacterModel.Gender = GetGenderIcon(character.Gender);
                        VnCharacterModel.BloodType = character.BloodType;
                        VnCharacterModel.Birthday = character.Birthday;
                        VnCharacterModel.Description = ConvertTextBBcode.ConvertText(character.Description);
                        if (string.IsNullOrEmpty(character.Aliases))
                            VnCharacterModel.Aliases = string.Empty;
                        else
                            VnCharacterModel.Aliases = character.Aliases.Contains(",")
                                ? character.Aliases.Replace(",", ", ")
                                : character.Aliases;

                        string path = $@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}\{Path.GetFileName(character.ImageLink)}";
                        if (!string.IsNullOrEmpty(path))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = stream;
                                bitmap.EndInit();
                                bitmap.Freeze();
                                VnCharacterModel.Image = bitmap;
                            }
                        }                        
                        VnCharacterModel.Bust = character.Bust.ToString();
                        VnCharacterModel.Waist = character.Waist.ToString();
                        VnCharacterModel.Hip = character.Hip.ToString();
                        VnCharacterModel.Height = character.Height.ToString();
                        VnCharacterModel.Weight = character.Weight.ToString();


                        LoadTraits();
                    }                    
                }
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }            
        }

        private void LoadTraits()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    List<TraitModel> traits = new List<TraitModel>();
                    var traitsWithParent = new Dictionary<TraitModel, List<string>>();
                    var traitArr = context.VnCharacterTraits
                        .Where(x => x.CharacterId == _characterId && x.SpoilerLevel < Globals.MaxSpoiler).Select(x => x.TraitId)
                        .ToArray();
                    traits.AddRange(traitArr.Select(trait => new TraitModel(Convert.ToInt32(trait), _TraitService)));


                    foreach (var trait in traits)
                    {
                        TraitModel parenttrait = _TraitService.GetLastParentTrait(trait);

                        if (traitsWithParent.Keys.Any(x => x.Name == parenttrait.Name))
                        {
                            traitsWithParent[traitsWithParent.Keys.First(x => x.Name == parenttrait.Name)].Add(trait.Name);
                        }
                        else
                        {
                            traitsWithParent.Add(parenttrait, new List<string>() { trait.Name });
                        }
                    }

                    string[] rootTraits = traitsWithParent.Select(parent => parent.Key.Name).ToArray();

                    foreach (var mainTrait in rootTraits)
                    {
                        var menuItem = new MenuItem() { Header = mainTrait };
                        var childTraitList = traitsWithParent.Where(x => x.Key.Name == mainTrait).Select(x => x.Value)
                            .First();

                        foreach (var trait in childTraitList)
                        {
                            menuItem.Items.Add(new MenuItem() { Header = trait });
                        }
                        TraitCollection.Add(menuItem);
                    }
                }
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
        }

        private void CheckMenuItemName(object obj)
        {
            try
            {
                MenuItem menuItem = (MenuItem)obj;
                if (obj == null) return;
                if (obj.GetType() == typeof(MenuItem))
                {
                    string name = menuItem.Header.ToString();
                    SelectedTrait = name;
                    if (menuItem.Parent != null)
                    {
                        if (menuItem.Parent.GetType() == typeof(MenuItem))
                        {
                            DatabaseContext context = new DatabaseContext();
                            if (SelectedTrait != "Traits")
                            {
                                BindTraitDescription();
                            }
                            else
                            {
                                SelectedTrait = String.Empty;
                            }
                            context.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }

        }

        private void BindTraitDescription()
        {
            if (SelectedTraitIndex >= 0)
                try
                {
                    using (var context = new DatabaseContext())
                    {
                        var desc = context.VnTraitData.Where(x => x.Name == SelectedTrait).Select(x => x.Description)
                            .FirstOrDefault();
                        TraitDescription = ConvertTextBBcode.ConvertText(desc);
                    }
                }
                catch (Exception ex)
                {
                    Globals.Logger.Error(ex);
                    throw;
                }
        }

        private String GetGenderIcon(string gender)
        {
            switch (gender)
            {
                case "Female":
                    GenderColor = "#CF0067";
                    return "M12,4A6,6 0 0,1 18,10C18,12.97 15.84,15.44 13,15.92V18H15V20H13V22H11V20H9V18H11V15.92C8.16,15.44 6,12.97 6,10A6,6 0 0,1 12,4M12,6A4,4 0 0,0 8,10A4,4 0 0,0 12,14A4,4 0 0,0 16,10A4,4 0 0,0 12,6Z";
                case "Male":
                    GenderColor = "#0067CF";
                    return
                        "M9,9C10.29,9 11.5,9.41 12.47,10.11L17.58,5H13V3H21V11H19V6.41L13.89,11.5C14.59,12.5 15,13.7 15,15A6,6 0 0,1 9,21A6,6 0 0,1 3,15A6,6 0 0,1 9,9M9,11A4,4 0 0,0 5,15A4,4 0 0,0 9,19A4,4 0 0,0 13,15A4,4 0 0,0 9,11Z";
                case "Both":
                    GenderColor = "#8A40D5";
                    return
                        "M17.58,4H14V2H21V9H19V5.41L15.17,9.24C15.69,10.03 16,11 16,12C16,14.42 14.28,16.44 12,16.9V19H14V21H12V23H10V21H8V19H10V16.9C7.72,16.44 6,14.42 6,12A5,5 0 0,1 11,7C12,7 12.96,7.3 13.75,7.83L17.58,4M11,9A3,3 0 0,0 8,12A3,3 0 0,0 11,15A3,3 0 0,0 14,12A3,3 0 0,0 11,9Z";
                default:
                    return null;
            }            
        }


    }
}
