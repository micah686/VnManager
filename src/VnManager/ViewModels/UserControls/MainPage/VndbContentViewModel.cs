using System;
using System.Collections.Generic;
using System.Text;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.UserControls.MainPage
{
    public class VndbContentViewModel: Screen
    {
        private Guid _userDataId;

        public Guid UserDataId
        {
            get => _userDataId;
            set
            {
                if (_userDataId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                {
                    _userDataId = value;
                }
            }
        }

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public VndbContentViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;

        }

        internal void SetUserDataId(Guid guid)
        {
            UserDataId = guid;
        }
    }
}
