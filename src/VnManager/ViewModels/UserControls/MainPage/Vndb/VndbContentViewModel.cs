using System;
using System.Collections.Generic;
using System.Text;
using Stylet;
using StyletIoC;

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
        internal void SetUserDataId(Guid guid)
        {
            UserDataId = guid;
        }
        #endregion
        public static VndbContentViewModel Instance { get; private set; }
        private readonly IContainer _container;

        public VndbContentViewModel(IContainer container)
        {
            _container = container;
            //LoadContent();
        }

        protected override void OnViewLoaded()
        {
            LoadContent();
            Instance ??= this;
        }

        private void LoadContent()
        {
            var vm = _container.Get<VndbInfoViewModel>();
            ActivateItem(vm);
            vm.SetUserDataId(UserDataId);
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
