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
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.TagTrait;


namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbCharactersViewModel: Screen
    {
        private IReadOnlyCollection<KeyValuePair<int, string>> _allCharacterNamesCollection;
        public BindableCollection<KeyValuePair<int, string>> CharacterNamesCollection { get; private set; }= new BindableCollection<KeyValuePair<int, string>>();
        public string CharacterNameSearch { get; set; }
        public int SelectedCharacterIndex { get; set; }
        private int _characterId = -1;
        private bool _finishedLoad = false;

        #region CharacterData
        public BitmapSource CharacterImage { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string BloodType { get; set; }
        public string Birthday { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string BustWaistHips { get; set; }

        public List<Inline> Description { get; set; }
        


        #endregion

        public PackIconMaterialKind GenderIcon { get; set; }
        public Brush GenderColorBrush { get; set; }


        protected override void OnViewLoaded()
        {
            PopulateCharacterList();
            
        }

        private void PopulateCharacterList()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbCharacterData = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.Instance.VnId).ToArray();
                List<KeyValuePair<int, string>> list = dbCharacterData.Select(x => new KeyValuePair<int, string>((int) x.CharacterId, x.Name))
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
            if(!_finishedLoad)return;
            if (SelectedCharacterIndex != -1)
            {
                _characterId = CharacterNamesCollection[SelectedCharacterIndex].Key;
            }
            UpdateCharacterData();
        }

        private void UpdateCharacterData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var charInfo = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query()
                    .Where(x => x.CharacterId == _characterId).FirstOrDefault();
                var imagePath = $@"{App.AssetDirPath}\sources\vndb\images\characters\{VndbContentViewModel.Instance.VnId}\{Path.GetFileName(charInfo.ImageLink)}";
                CharacterImage = ImageHelper.CreateBitmapFromPath(imagePath);

                Name = charInfo.Name;
                OriginalName = charInfo.Original ?? string.Empty;

                SetGenderIcon(charInfo.Gender);
                Birthday = charInfo.Birthday ?? string.Empty;
                

                if (!string.IsNullOrEmpty(charInfo.BloodType))
                {
                    BloodType = $"Blood Type: {charInfo.BloodType}";
                }

                if (!charInfo.Height.Equals(0))
                {
                    Height = $"Height: {charInfo.Height}";
                }

                if (!charInfo.Weight.Equals(0))
                {
                    Weight = $"Weight: {charInfo.Weight}";
                }

                SetBustWidthHeight(charInfo);

                Description = BBCodeHelper.Helper(charInfo.Description);
            }
            GetTraits();
        }

        private void SetBustWidthHeight(VnCharacterInfo info)
        {
            var header = App.ResMan.GetString("BustWaistHips");
            
            string bust = info.Bust == null || info.Bust == 0 ? "??" : info.Bust.ToString();
            string waist = info.Waist == null || info.Waist == 0 ? "??" : info.Waist.ToString();
            string hips = info.Hip == null || info.Hip == 0 ? "??" : info.Hip.ToString();

            if (bust.Equals("??") && waist.Equals("??") && hips.Equals("??"))
            {
                BustWaistHips = String.Empty;
            }
            else
            {
                var value = $"{header} {bust}-{waist}-{hips}";
                BustWaistHips = value;
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
            List<VnCharacterTraits> traitList= new List<VnCharacterTraits>();
            List<VnTraitData> traitDump = new List<VnTraitData>();

            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                traitList = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString()).Query()
                    .Where(x => x.SpoilerLevel <= App.UserSettings.SettingsVndb.Spoiler).ToList();
                traitDump = db.GetCollection<VnTraitData>(DbVnDump.VnDump_TraitData.ToString()).Query().ToList();
            }

            var foo = traitDump.Where(x => x.Parents.Length > 2).ToList();
            List<TraitInfo> traitInfoList = new List<TraitInfo>();
            foreach (var trait in traitList)
            {
                var traitName = traitDump.FirstOrDefault(x => x.TraitId == trait.TraitId)?.Name;
                var parent = GetParentTrait(trait.TraitId, traitDump);
                if (parent == traitName)
                {
                    parent = null;
                }
                traitInfoList.Add(new TraitInfo {TraitId = trait.TraitId, TraitName = traitName, ParentTraitName = parent});
            }
        }


        private string GetParentTrait(uint traitId, List<VnTraitData> traitDump)
        {
            var traitData = traitDump.FirstOrDefault(x => x.TraitId == traitId);

            while (traitData != null && traitData.Parents.Length > 0)
            {
                traitData = traitDump.FirstOrDefault(x => x.TraitId == traitData.Parents.Last());
            }
            return traitData?.Name;
        }

        private struct TraitInfo
        {
            public uint TraitId;
            public string TraitName;
            public string ParentTraitName;
        }
        
    }
}
