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
                var dbInfoLinks = db.GetCollection<VnInfoLinks>(DbVnInfo.VnInfo_Links.ToString());
                var dbInfoRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString());
                var dbInfoScreens = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString());
                var dbInfoTags = db.GetCollection<VnInfoTags>(DbVnInfo.VnInfo_Tags.ToString());


                

                

                var dbCharacter = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString());
                var dbCharacterTraits = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString());


                var vnid = ModifyGameHostViewModel.SelectedGame.GameId.Value;
                var charIds = dbCharacter.Query().Where(x => x.VnId == vnid).Select(x => x.CharacterId).ToList();
                

                #region Info
                dbInfo.DeleteMany(x => x.VnId == vnid);
                dbInfoLinks.DeleteMany(x => x.VnId == vnid);
                dbInfoRelations.DeleteMany(x => x.VnId == vnid);
                dbInfoScreens.DeleteMany(x => x.VnId == vnid);
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
                dbCharacterTraits.DeleteMany(x => charDeleteIds.Contains(x.CharacterId));

                #endregion

                





                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                dbUserData.DeleteMany(x => x.Id == ModifyGameHostViewModel.SelectedGame.Id);



            }
        }
    }
}
