using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;


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
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string BloodType { get; set; }
        public string Birthday { get; set; }
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
                Name = charInfo.Name;
                OriginalName = charInfo.Original;
                SetGenderIcon(charInfo.Gender);
                BloodType = charInfo.BloodType;
                Birthday = charInfo.Birthday;
                

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
            }
        }
        
    }
}
