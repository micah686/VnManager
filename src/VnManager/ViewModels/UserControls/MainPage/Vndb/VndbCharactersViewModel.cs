using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using MahApps.Metro.IconPacks;
using Stylet;
using VndbSharp.Models.Common;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.Models.Settings;


namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbCharactersViewModel: Screen
    {
        #region CharacterListProperties
        private IReadOnlyCollection<KeyValuePair<int, string>> _allCharacterNamesCollection;
        public BindableCollection<KeyValuePair<int, string>> CharacterNamesCollection { get; private set; } = new BindableCollection<KeyValuePair<int, string>>();
        public string CharacterNameSearch { get; set; }
        private bool _finishedLoad = false;
        #endregion


        public int SelectedCharacterIndex { get; set; }
        private int _characterId = -1;

        #region CharacterData
        public BitmapSource CharacterImage { get; set; }
        public string Name { get; set; }
        public Tuple<string, Visibility> OriginalName { get; set; }
        public Tuple<string, Visibility> BloodType { get; set; } 
        public Tuple<string, Visibility> Birthday { get; set; }
        public Tuple<string, Visibility> Height { get; set; }
        public Tuple<string, Visibility> Weight { get; set; }
        public Tuple<string, Visibility> BustWaistHips { get; set; }

        public List<Inline> Description { get; set; }
        
        public BindableCollection<TraitBinding> TraitCollection { get; set; } = new BindableCollection<TraitBinding>();

        #endregion

        public PackIconMaterialKind GenderIcon { get; set; }
        public Brush GenderColorBrush { get; set; }

        protected override void OnViewLoaded()
        {
            if(CharacterNamesCollection.Count >0)return;
            PopulateCharacterList();

            if (CharacterNamesCollection.Count > 0)
            {
                SelectedCharacterIndex = 0;
            }
            SetupDefaultVisProps();
        }

        #region CharacterListMethods
        private void PopulateCharacterList()
        {
            CharacterNamesCollection.Clear();
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbCharacterData = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.VnId).ToArray();
                List<KeyValuePair<int, string>> list = dbCharacterData.Select(x => new KeyValuePair<int, string>((int)x.CharacterId, x.Name))
                    .ToList();
                CharacterNamesCollection.AddRange(list);
                _allCharacterNamesCollection = list;
                SelectedCharacterIndex = -1;
                _finishedLoad = true;
            }

        }

        public void SearchCharacters()
        {
            if (CharacterNameSearch.Length < 1 && CharacterNamesCollection.Equals(_allCharacterNamesCollection)) return;

            if (CharacterNameSearch.Length < 1)
            {
                CharacterNamesCollection.Clear();
                CharacterNamesCollection.AddRange(_allCharacterNamesCollection);
                SelectedCharacterIndex = -1;
                return;
            }

            List<KeyValuePair<int, string>> validCharacters = _allCharacterNamesCollection.Where(x => x.Value.ToUpperInvariant().Contains(CharacterNameSearch.ToUpperInvariant())).ToList();
            CharacterNamesCollection.Clear();
            CharacterNamesCollection.AddRange(validCharacters);
        }

        public void CharacterSelectionChanged()
        {
            if (!_finishedLoad) return;
            if (SelectedCharacterIndex != -1)
            {
                _characterId = CharacterNamesCollection[SelectedCharacterIndex].Key;
            }
            UpdateCharacterData();
        }
        #endregion

        private void SetupDefaultVisProps()
        {
            var defTuple = new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            OriginalName = defTuple;
            Birthday = defTuple;
            BloodType = defTuple;
            Height = defTuple;
            Weight = defTuple;
            BustWaistHips = defTuple;
        }



        private void UpdateCharacterData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var charInfo = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query()
                    .Where(x => x.CharacterId == _characterId).FirstOrDefault();
                var imagePath = $@"{App.AssetDirPath}\sources\vndb\images\characters\{VndbContentViewModel.VnId}\{Path.GetFileName(charInfo.ImageLink)}";
                CharacterImage = ImageHelper.CreateBitmapFromPath(imagePath);

                SetupDefaultVisProps();

                Name = charInfo.Name;
                SetGenderIcon(charInfo.Gender);
                if (!string.IsNullOrEmpty(charInfo.Original))
                {
                    OriginalName = new Tuple<string, Visibility>(charInfo.Original, Visibility.Visible);
                }

                if (!string.IsNullOrEmpty(charInfo.Birthday))
                {
                    Birthday = new Tuple<string, Visibility>($"Birthday: {charInfo.Birthday}", Visibility.Visible);
                }

                if (!string.IsNullOrEmpty(charInfo.BloodType))
                {
                    BloodType = new Tuple<string, Visibility>($"Blood Type: {charInfo.BloodType}", Visibility.Visible);
                }

                if (!charInfo.Height.Equals(0))
                {
                    Height = new Tuple<string, Visibility>($"Height: {charInfo.Height}", Visibility.Visible);
                }

                if (!charInfo.Weight.Equals(0))
                {
                    Weight = new Tuple<string, Visibility>($"Weight: {charInfo.Weight}", Visibility.Visible);
                }

                SetBustWidthHeight(charInfo);

                Description = BBCodeHelper.Helper(charInfo.Description);
            }
            GetTraits();
            SetParentTraitMargin();
        }

        private void SetBustWidthHeight(VnCharacterInfo info)
        {
            var header = App.ResMan.GetString("BustWaistHips");
            
            string bust = info.Bust == null || info.Bust == 0 ? "??" : info.Bust.ToString();
            string waist = info.Waist == null || info.Waist == 0 ? "??" : info.Waist.ToString();
            string hips = info.Hip == null || info.Hip == 0 ? "??" : info.Hip.ToString();

            var isAllValid = !bust.Equals("??") && !waist.Equals("??") && !hips.Equals("??");

            if (isAllValid)
            {
                var value = $"{header} {bust}-{waist}-{hips}";
                BustWaistHips = new Tuple<string, Visibility>(value, Visibility.Visible);
            }
        }

        private void SetGenderIcon(string gender)
        {
            switch (gender)
            {
                case "Female":
                    GenderIcon = PackIconMaterialKind.GenderFemale;
                    GenderColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#CF0067"));
                    break;
                case "Male":
                    GenderIcon = PackIconMaterialKind.GenderMale;
                    GenderColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0067CF"));
                    break;
                case "Both":
                    GenderIcon = PackIconMaterialKind.GenderMaleFemale;
                    GenderColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8A40D5"));
                    break;
                default:
                    GenderIcon = PackIconMaterialKind.None;
                    break;
            }
        }

        private void GetTraits()
        {
            TraitCollection.Clear();
            List<VnCharacterTraits> traitList;
            List<VnTraitData> traitDump;

            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                traitList = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString()).Query()
                    .Where(x => x.CharacterId == _characterId && x.SpoilerLevel <= App.UserSettings.SettingsVndb.Spoiler).ToList();
                traitDump = db.GetCollection<VnTraitData>(DbVnDump.VnDump_TraitData.ToString()).Query().ToList();
            }


            List<(string Parent, string Child)> traitInfoList = new List<(string Parent, string Child)>();
            foreach (var trait in traitList)
            {
                var traitName = traitDump.FirstOrDefault(x => x.TraitId == trait.TraitId)?.Name;
                var parent = GetParentTrait(trait.TraitId, traitDump);
                if (parent == traitName)
                {
                    parent = null;
                }
                traitInfoList.Add((parent,traitName));
            }

            var groupedTraits = traitInfoList.GroupBy(x => x.Parent).ToList();
            //creates a List<string> from the children of the named tuple. Then adds the parent and children list to the TraitBinding list
            List<TraitBinding> bindingTraits = (from item in groupedTraits let childList = item.Select(x => x.Child).ToList() 
                select new TraitBinding {Parent = item.Key, Children = childList }).ToList();
            bindingTraits = bindingTraits.OrderBy(x => x.Parent).ToList();

            bindingTraits.RemoveAll(x =>
                x.Parent.Contains("Sexual") && App.UserSettings.MaxSexualRating < SexualRating.Explicit);
            
            TraitCollection.AddRange(bindingTraits);
            SetParentTraitMargin();
        }


        private static string GetParentTrait(uint traitId, List<VnTraitData> traitDump)
        {
            var traitData = traitDump.FirstOrDefault(x => x.TraitId == traitId);

            while (traitData != null && traitData.Parents.Length > 0)
            {
                traitData = traitDump.FirstOrDefault(x => x.TraitId == traitData.Parents.Last());
            }
            return traitData?.Name;
        }


        private void SetParentTraitMargin()
        {
            var currentTraits = TraitCollection;

            var parents = currentTraits.Select(x => x.Parent);
            var stringSizes = parents.Select(MeasureStringSize.GetMaxStringWidth).ToList();
            var largestSize = stringSizes.OrderByDescending(x => x).FirstOrDefault();
            var padding = 30;
            var maxValue = largestSize + padding;

            foreach (var traitBinding in TraitCollection)
            {
                var parentSize = MeasureStringSize.GetMaxStringWidth(traitBinding.Parent);
                if (parentSize < maxValue)
                {
                    var difference = maxValue - parentSize;
                    traitBinding.ParentMargin = new Thickness(difference,0,0,0);
                }

            }

            TraitCollection = currentTraits;
            
        }

    }

    public class TraitBinding
    {
        public string Parent { get; set; }
        public List<string> Children { get; set; }
        public Thickness ParentMargin { get; set; }
    }
}
