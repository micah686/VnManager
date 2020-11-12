using System;
using System.Collections.Generic;
using System.Text;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbContentViewModel: Conductor<IScreen>.Collection.OneActive
    {
        #region UserGuid
        private Guid _userDataId;
        public Guid UserDataId
        {
            get => _userDataId;
            private set
            {
                if (_userDataId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                {
                    _userDataId = value;
                }
            }
        }

        #endregion
        public int SelectedTabId { get; set; }
        public static VndbContentViewModel Instance { get; internal set; }
        
        internal int VnId;


        public VndbContentViewModel()
        {
            
            //LoadContent();
            var vInfo = new VndbInfoViewModel {DisplayName = App.ResMan.GetString("Main")};
            var vChar = new VndbCharactersViewModel { DisplayName = App.ResMan.GetString("Characters") };
            var vScreen = new VndbScreensViewModel { DisplayName = App.ResMan.GetString("Screenshots") };

            Items.Add(vInfo);
            Items.Add(vChar);
            Items.Add(vScreen);
        }

        protected override void OnViewLoaded()
        {
            Instance ??= this;
        }


        internal void SetUserDataId(Guid guid)
        {
            UserDataId = guid;
            SetGameId();
        }

        private void SetGameId()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                .Where(x => x.Id == UserDataId).FirstOrDefault();
            if (dbUserData != null)
            {
                VnId = dbUserData.GameId;
            }
        }

        /// <summary>
        /// Cleanup Vndb views when pressing the close button
        /// Sets ContentViewModel Instance to null
        /// </summary>
        internal static void Cleanup()
        {
            Instance = null;
            
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            Cleanup();
        }
    }

}
