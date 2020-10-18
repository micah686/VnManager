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
    public class VndbContentViewModel: Conductor<Screen>
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
        public static VndbContentViewModel Instance { get; internal set; }
        private readonly IContainer _container;
        internal int VnId;


        public VndbContentViewModel(IContainer container)
        {
            _container = container;
            //LoadContent();
        }

        protected override void OnViewLoaded()
        {
            ActivateVnInfo();
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




        internal void ActivateVnScreenshots()
        {
            var vm = _container.Get<VndbScreensViewModel>();
            ActivateItem(vm);
        }

        internal void ActivateVnCharacters()
        {
            var vm = _container.Get<VndbCharactersViewModel>();
            ActivateItem(vm);
        }

        internal void ActivateVnInfo()
        {
            var vm = _container.Get<VndbInfoViewModel>();
            ActivateItem(vm);
        }
    }

}
