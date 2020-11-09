using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbCharactersViewModel: Screen
    {
        private IReadOnlyCollection<string> _allCharacterNamesCollection= new List<string>();
        public BindableCollection<string> CharacterNamesCollection { get; private set; } = new BindableCollection<string>();
        public string CharacterNameSearch { get; set; }
        public string TestBnd { get; set; }

        protected override void OnViewLoaded()
        {
            PopulateCharacterList();

            TestBnd = "untouched";
        }


        private void PopulateCharacterList()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbCharacterData = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.Instance.VnId).Select(x => x.Name).ToArray();
                CharacterNamesCollection.AddRange(dbCharacterData);
                _allCharacterNamesCollection = dbCharacterData;
            }
        }

        public void SearchCharacters()
        {
            if(CharacterNameSearch.Length < 1 && CharacterNamesCollection.Equals(_allCharacterNamesCollection)) return;

            if (CharacterNameSearch.Length < 1)
            {
                CharacterNamesCollection.Clear();
                CharacterNamesCollection.AddRange(_allCharacterNamesCollection);
                return;
            }

            if(CharacterNameSearch.Length <1) return;
            List<string> validCharacters = _allCharacterNamesCollection.Where(characterName => characterName.ToUpperInvariant().Contains(CharacterNameSearch.ToUpperInvariant())).ToList();
            CharacterNamesCollection.Clear();
            CharacterNamesCollection.AddRange(validCharacters);
        }



        public static void ShowInfo()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnInfo();
        }

        public static void ShowScreens()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnScreenshots();
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            VndbContentViewModel.Instance.Cleanup();
        }

    }
}
