// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using MahApps.Metro.IconPacks;
using Sentry;
using Stylet;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;


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
        private int _characterId = 0;

        #region CharacterData
        public BitmapSource CharacterImage { get; set; }
        public string Name { get; set; }
        
        public Visibility TraitHeaderVisibility { get; set; }
        public Tuple<string, Visibility> OriginalName { get; set; }
        public Tuple<string, Visibility> Age { get; set; }
        public Tuple<string, Visibility> BloodType { get; set; } 
        public Tuple<string, Visibility> Birthday { get; set; }
        public Tuple<string, Visibility> Height { get; set; }
        public Tuple<string, Visibility> Weight { get; set; }
        public Tuple<string, Visibility> BustWaistHips { get; set; }

        public Inline[] Description { get; set; }
        
        public BindableCollection<TagTraitBinding> TraitCollection { get; } = new BindableCollection<TagTraitBinding>();

        #endregion

        public PackIconMaterialKind GenderIcon { get; set; }
        public Brush GenderColorBrush { get; set; }

        protected override void OnViewLoaded()
        {
            _finishedLoad = false;
            if(CharacterNamesCollection.Count >0)
            {
                return;
            }
            PopulateCharacterList();

            if (CharacterNamesCollection.Count > 0)
            {
                SelectedCharacterIndex = 0;
            }
            SetupDefaultVisProps();
            _finishedLoad = true;
            
            CharacterSelectionChanged();
        }

        #region CharacterListMethods
        /// <summary>
        /// Fill character list with characters associated with the game
        /// </summary>
        private void PopulateCharacterList()
        {
            try
            {
                CharacterNamesCollection.Clear();
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbCharacterData = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query()
                        .Where(x => x.VnId == VndbContentViewModel.VnId).ToArray();
                    List<KeyValuePair<int, string>> list = dbCharacterData.Select(x => new KeyValuePair<int, string>((int)x.CharacterId, x.Name))
                        .ToList();
                    CharacterNamesCollection.AddRange(list);
                    _allCharacterNamesCollection = list;
                    SelectedCharacterIndex = 0;
                }
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to fill Vndb Character List");
                SentrySdk.CaptureException(e);
                throw;
            }
        }

        /// <summary>
        /// Search for a specified character
        /// <see cref="SearchCharacters"/>
        /// </summary>
        public void SearchCharacters()
        {
            try
            {
                if (CharacterNameSearch.Length < 1 && CharacterNamesCollection.Equals(_allCharacterNamesCollection))
                {
                    return;
                }

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
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to search for selected character");
                SentrySdk.CaptureException(e);
            }
        }

        /// <summary>
        /// Update data when character changes
        /// </summary>
        public void CharacterSelectionChanged()
        {
            try
            {
                if (!_finishedLoad)
                {
                    return;
                }
                if (SelectedCharacterIndex != -1)
                {
                    _characterId = CharacterNamesCollection[SelectedCharacterIndex].Key;
                }
                UpdateCharacterData();
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to search for Change character data on SelectionChanged");
                SentrySdk.CaptureException(e);
            }
        }
        #endregion

        /// <summary>
        /// Sets default visibility on properties
        /// </summary>
        private void SetupDefaultVisProps()
        {
            var defTuple = new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            OriginalName = defTuple;
            Birthday = defTuple;
            Age = defTuple;
            BloodType = defTuple;
            Height = defTuple;
            Weight = defTuple;
            BustWaistHips = defTuple;
        }


        /// <summary>
        /// Update character data
        /// </summary>
        private void UpdateCharacterData()
        {
            try
            {
                if(_characterId == -1)
                {
                    return;
                }
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
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
                        Birthday = new Tuple<string, Visibility>($"{App.ResMan.GetString("BirthdayColon")} {charInfo.Birthday}", Visibility.Visible);
                    }
                    if (!string.IsNullOrEmpty(charInfo.Age))
                    {
                        Birthday = new Tuple<string, Visibility>($"{App.ResMan.GetString("AgeColon")} {charInfo.Age}", Visibility.Visible);
                    }

                    if (!string.IsNullOrEmpty(charInfo.BloodType))
                    {
                        BloodType = new Tuple<string, Visibility>($"{App.ResMan.GetString("BloodTypeColon")} {charInfo.BloodType}", Visibility.Visible);
                    }

                    if (!charInfo.Height.Equals(0))
                    {
                        Height = new Tuple<string, Visibility>($"{App.ResMan.GetString("HeightColon")} {charInfo.Height}", Visibility.Visible);
                    }

                    if (!charInfo.Weight.Equals(0))
                    {
                        Weight = new Tuple<string, Visibility>($"{App.ResMan.GetString("WeightColon")} {charInfo.Weight}", Visibility.Visible);
                    }

                    SetBustWaistHips(charInfo);

                    Description = BBCodeHelper.Helper(charInfo.Description);
                }

                UpdateTraits();
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to update character data");
                SentrySdk.CaptureException(e);
            }
        }

        /// <summary>
        /// Fill traitCollection with character traits
        /// </summary>
        private void UpdateTraits()
        {
            TraitCollection.Clear();
            TraitCollection.AddRange(VndbTagTraitHelper.GetTraits(_characterId));
            TraitHeaderVisibility = TraitCollection.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
        }
        
        /// <summary>
        /// Sets the bust, waist, and hips of the character
        /// </summary>
        /// <param name="info"></param>
        private void SetBustWaistHips(VnCharacterInfo info)
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

        /// <summary>
        /// Sets the Gender icon of the character
        /// </summary>
        /// <param name="gender"></param>
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

    }
}
