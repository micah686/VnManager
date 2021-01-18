using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AdysTech.CredentialManager;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db.Vndb.Producer;
using VnManager.Models.Db.Vndb.Release;
using VnManager.Models.Db.Vndb.Staff;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameDeleteViewModel: Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        public ModifyGameDeleteViewModel(IWindowManager windowManager, IDialogService dialogService)
        {
            DisplayName = App.ResMan.GetString("DeleteGame");
            _windowManager = windowManager;
            _dialogService = dialogService;
            
        }

        public void DeleteGame()
        {
            var result = _windowManager.ShowMessageBox(App.ResMan.GetString("DeleteGameCheck"), App.ResMan.GetString("DeleteGame"),
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                DeleteData();

            }
        }

        private void DeleteData()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {

                
                var dbInfo = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString());
                var dbInfoAnime = db.GetCollection<VnInfoAnime>(DbVnInfo.VnInfo_Anime.ToString());
                var dbInfoLinks = db.GetCollection<VnInfoLinks>(DbVnInfo.VnInfo_Links.ToString());
                var dbInfoRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString());
                var dbInfoScreens = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString());
                var dbInfoStaff = db.GetCollection<VnInfoStaff>(DbVnInfo.VnInfo_Staff.ToString());
                var dbInfoTags = db.GetCollection<VnInfoTags>(DbVnInfo.VnInfo_Tags.ToString());

                var dbProducer = db.GetCollection<VnProducer>(DbVnProducer.VnProducer.ToString());
                var dbProducerLinks = db.GetCollection<VnProducerLinks>(DbVnProducer.VnProducer_Links.ToString());
                var dbProducerRelations = db.GetCollection<VnProducerRelations>(DbVnProducer.VnProducer_Relations.ToString());

                var dbRelease = db.GetCollection<VnRelease>(DbVnRelease.VnReleases.ToString());
                var dbReleaseMedia = db.GetCollection<VnReleaseMedia>(DbVnRelease.VnRelease_Media.ToString());
                var dbReleaseProducers = db.GetCollection<VnReleaseProducers>(DbVnRelease.VnRelease_Producers.ToString());
                var dbReleaseVns = db.GetCollection<VnReleaseVn>(DbVnRelease.VnRelease_Vns.ToString());

                var dbStaff = db.GetCollection<VnStaff>(DbVnStaff.VnStaff.ToString());
                var dbStaffAliases = db.GetCollection<VnStaffAliases>(DbVnStaff.VnStaff_Aliases.ToString());
                var dbStaffVns = db.GetCollection<VnStaffVns>(DbVnStaff.VnStaff_Vns.ToString());
                var dbStaffVoiced = db.GetCollection<VnStaffVoiced>(DbVnStaff.VnStaff_Voiced.ToString());

                var dbCharacter = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString());
                var dbCharacterInstances = db.GetCollection<VnCharacterInstances>(DbVnCharacter.VnCharacter_Instances.ToString());
                var dbCharacterTraits = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString());
                var dbCharacterVns = db.GetCollection<VnCharacterVns>(DbVnCharacter.VnCharacter_Vns.ToString());
                var dbCharacterVoiced = db.GetCollection<VnCharacterVoiced>(DbVnCharacter.VnCharacter_Voiced.ToString());


                var vnid = ModifyGameHostViewModel.SelectedGame.GameId.Value;
                var charIds = dbCharacter.Query().Where(x => x.VnId == vnid).Select(x => x.CharacterId).ToList();
                var releaseIds = dbRelease.Query().Where(x => x.VnId == vnid).Select(x => x.ReleaseId).ToList();
                var producerIds = dbReleaseProducers.Query().Where(x => releaseIds.Contains(x.ReleaseId)).Select(x => x.ProducerId).ToList();

                #region Info
                dbInfo.DeleteMany(x => x.VnId == vnid);
                dbInfoAnime.DeleteMany(x => x.VnId == vnid);
                dbInfoLinks.DeleteMany(x => x.VnId == vnid);
                dbInfoRelations.DeleteMany(x => x.VnId == vnid);
                dbInfoScreens.DeleteMany(x => x.VnId == vnid);
                dbInfoStaff.DeleteMany(x => x.VnId == vnid);
                dbInfoTags.DeleteMany(x => x.VnId == vnid);


                #endregion

                #region Character

                
                var charExclude = new List<uint>();
                foreach (var characterInfo in dbCharacter.FindAll())
                {
                    if (charIds.Contains(characterInfo.CharacterId) && characterInfo.VnId != vnid)
                    {
                        charExclude.Add(characterInfo.CharacterId);
                    }
                }

                var charDeleteIds = charIds.Except(charExclude).ToList();
                dbCharacter.DeleteMany(x => charDeleteIds.Contains(x.CharacterId));
                dbCharacterInstances.DeleteMany(x => charDeleteIds.Contains((uint) x.CharacterId));
                dbCharacterTraits.DeleteMany(x => charDeleteIds.Contains(x.CharacterId));
                dbCharacterVns.DeleteMany(x => charDeleteIds.Contains(x.CharacterId));
                dbCharacterVoiced.DeleteMany(x => charDeleteIds.Contains((uint) x.CharacterId));

                #endregion

                #region Releases

                
                
                dbRelease.DeleteMany(x => releaseIds.Contains(x.ReleaseId));
                dbReleaseMedia.DeleteMany(x => releaseIds.Contains(x.ReleaseId));
                dbReleaseVns.DeleteMany(x => releaseIds.Contains(x.ReleaseId));
                dbReleaseProducers.DeleteMany(x => releaseIds.Contains(x.ReleaseId));


                #endregion

                #region Producers

                dbProducer.DeleteMany(x => producerIds.Contains((uint) x.ProducerId));
                dbProducerLinks.DeleteMany(x => producerIds.Contains((uint) x.ProducerId));
                dbProducerRelations.DeleteMany(x => producerIds.Contains((uint) x.ProducerId));
                #endregion





                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                dbUserData.DeleteMany(x => x.Id == ModifyGameHostViewModel.SelectedGame.Id);



            }
        }
    }
}
