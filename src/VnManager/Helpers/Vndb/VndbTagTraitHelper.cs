using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AdysTech.CredentialManager;
using LiteDB;
using VndbSharp.Models.Common;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.Models.Settings;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

namespace VnManager.Helpers.Vndb
{
    public static class VndbTagTraitHelper
    {
        private const string _sexualString = "Sexual";
        #region Tags
        public static List<TagTraitBinding> GetTags(int vnId)
        {
            List<VnInfoTags> tagList;
            List<VnTagData> tagDump;

            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return new List<TagTraitBinding>();
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {

                tagList = db.GetCollection<VnInfoTags>(DbVnInfo.VnInfo_Tags.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.VnId).ToList();
                tagDump = db.GetCollection<VnTagData>(DbVnDump.VnDump_TagData.ToString()).Query().ToList();

                tagList = tagList.Where(t => t.Spoiler <= App.UserSettings.SettingsVndb.Spoiler).ToList();
                tagList = tagList.OrderByDescending(x => x.Spoiler).ToList();

            }

            var tagsWithParent = GetParentTags(tagList, tagDump).ToList();
            tagsWithParent.RemoveAll(x => x.Parent.Contains(_sexualString) && App.UserSettings.MaxSexualRating < SexualRating.Explicit);

            var noSpoilerTags = tagsWithParent.Where(x => x.Spoiler == SpoilerLevel.None).ToList();
            var minorSpoilerTags = tagsWithParent.Where(x => x.Spoiler == SpoilerLevel.Minor).ToList();
            var majorSpoilerTags = tagsWithParent.Where(x => x.Spoiler == SpoilerLevel.Major).ToList();

            var sexualTags = tagsWithParent.Where(x => x.Parent.Contains(_sexualString) && x.Spoiler < SpoilerLevel.Major).ToList();


            var tempList = (from tag in noSpoilerTags let colorText = Colors.WhiteSmoke.ToString(CultureInfo.InvariantCulture) select (tag.Parent, tag.Child, colorText)).ToList();
            tempList.AddRange(from tag in minorSpoilerTags let colorText = Colors.Gold.ToString(CultureInfo.InvariantCulture) select (tag.Parent, tag.Child, colorText));
            tempList.AddRange(from tag in majorSpoilerTags let colorText = Colors.Red.ToString(CultureInfo.InvariantCulture) select (tag.Parent, tag.Child, colorText));

            tempList = tempList.Except(tempList.Where(p => sexualTags.Any(c => c.Child == p.Child))).ToList();
            tempList.AddRange(from tag in sexualTags let colorText = Colors.HotPink.ToString(CultureInfo.InvariantCulture) select (tag.Parent, tag.Child, colorText));


            var tagBindingList = (from @group in tempList.GroupBy(x => x.Parent)
                let tuple = @group.Select(tag => new Tuple<string, string>(tag.Child, tag.colorText)).ToList()
                select new TagTraitBinding {Parent = @group.Key, Children = tuple}).OrderBy(x => x.Parent).ToList();
            return tagBindingList;
        }

        private static List<(string Parent, string Child, SpoilerLevel Spoiler)> GetParentTags(List<VnInfoTags> tagList, List<VnTagData> tagDump)
        {
            List<(string Parent, string Child, SpoilerLevel Spoiler)> tagInfoList = new List<(string Parent, string Child, SpoilerLevel Spoiler)>();
            foreach (var tag in tagList)
            {
                var tagName = tagDump.FirstOrDefault(x => x.TagId == tag.TagId)?.Name;
                if (string.IsNullOrEmpty(tagName))
                {
                    continue;
                }
                var tagData = tagDump.FirstOrDefault(x => x.TagId == tag.TagId);
                while (tagData != null && tagData.Parents.Length > 0)
                {
                    tagData = tagDump.FirstOrDefault(x => x.TagId == tagData.Parents.Last());
                }
                var parentTag = tagData?.Name;
                
                tagInfoList.Add((parentTag, tagName, tag.Spoiler));
            }
            return tagInfoList;
        }


        #endregion

        #region Traits
        public static List<TagTraitBinding> GetTraits(int characterId)
        {
            List<VnCharacterTraits> traitList;
            List<VnTraitData> traitDump;

            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return new List<TagTraitBinding>();
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {

                traitList = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString()).Query()
                    .Where(x => x.CharacterId == characterId).ToList();
                traitDump = db.GetCollection<VnTraitData>(DbVnDump.VnDump_TraitData.ToString()).Query().ToList();

                traitList = traitList.Where(t => t.SpoilerLevel <= App.UserSettings.SettingsVndb.Spoiler).ToList();
                traitList = traitList.OrderByDescending(x => x.SpoilerLevel).ToList();

            }

            var traitsWithParent = GetParentTraits(traitList, traitDump).ToList();
            traitsWithParent.RemoveAll(x => x.Parent.Contains(_sexualString) && App.UserSettings.MaxSexualRating < SexualRating.Explicit);

            var noSpoilerTraits = traitsWithParent.Where(x => x.Spoiler == SpoilerLevel.None).ToList();
            var minorSpoilerTraits = traitsWithParent.Where(x => x.Spoiler == SpoilerLevel.Minor).ToList();
            var majorSpoilerTraits = traitsWithParent.Where(x => x.Spoiler == SpoilerLevel.Major).ToList();

            var sexualTraits = traitsWithParent.Where(x => x.Parent.Contains(_sexualString) && x.Spoiler < SpoilerLevel.Major).ToList();


            var tempList = (from trait in noSpoilerTraits let colorText = Colors.WhiteSmoke.ToString(CultureInfo.InvariantCulture) select (trait.Parent, trait.Child, colorText)).ToList();
            tempList.AddRange(from trait in minorSpoilerTraits let colorText = Colors.Gold.ToString(CultureInfo.InvariantCulture) select (trait.Parent, trait.Child, colorText));
            tempList.AddRange(from trait in majorSpoilerTraits let colorText = Colors.Crimson.ToString(CultureInfo.InvariantCulture) select (trait.Parent, trait.Child, colorText));

            tempList = tempList.Except(tempList.Where(p => sexualTraits.Any(c => c.Child == p.Child))).ToList();
            tempList.AddRange(from trait in sexualTraits let colorText = Colors.HotPink.ToString(CultureInfo.InvariantCulture) select (trait.Parent, trait.Child, colorText));


            var traitBindingList = (from @group in tempList.GroupBy(x => x.Parent)
                let tuple = @group.Select(trait => new Tuple<string, string>(trait.Child, trait.colorText)).ToList()
                select new TagTraitBinding {Parent = @group.Key, Children = tuple}).OrderBy(x => x.Parent).ToList();
            return traitBindingList;
        }
        
        private static List<(string Parent, string Child, SpoilerLevel Spoiler)> GetParentTraits(List<VnCharacterTraits> traitList, List<VnTraitData> traitDump)
        {
            List<(string Parent, string Child, SpoilerLevel Spoiler)> traitInfoList = new List<(string Parent, string Child, SpoilerLevel Spoiler)>();
            foreach (var trait in traitList)
            {
                var traitName = traitDump.FirstOrDefault(x => x.TraitId == trait.TraitId)?.Name;
                if (string.IsNullOrEmpty(traitName))
                {
                    continue;
                }
                var traitData = traitDump.FirstOrDefault(x => x.TraitId == trait.TraitId);
                while (traitData != null && traitData.Parents.Length > 0)
                {
                    traitData = traitDump.FirstOrDefault(x => x.TraitId == traitData.Parents.Last());
                }
                var parentTag = traitData?.Name;
                
                traitInfoList.Add((parentTag, traitName, trait.SpoilerLevel));
            }
            return traitInfoList;
        }

        #endregion

    }


    public class TagTraitBinding
    {
        public string Parent { get; set; }
        /// <summary>
        /// Tuple is NameOfChild, ColorInHex
        /// </summary>
        public List<Tuple<string, string>> Children { get; set; }
    }
}
