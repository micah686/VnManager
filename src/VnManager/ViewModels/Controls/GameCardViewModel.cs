using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.ModifyGame;
using VnManager.ViewModels.UserControls.MainPage.Vndb;

//using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.Controls
{
    public class GameCardViewModel: Screen
    {
        private UserDataGames _selectedGame;
        
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public GameCardViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
        }
        #region CoverImage
        private BindingImage _coverImage;
        public BindingImage CoverImage
        {
            get => _coverImage;
            set
            {
                if (value.IsNsfw == false || ShouldDisplayNsfwContent)
                {
                    _coverImage = value;
                    SetAndNotify(ref _coverImage, value);
                }
                else
                {
                    value.Image = ImageHelper.BlurImage(value.Image, 10);
                    SetAndNotify(ref _coverImage, value);

                }
            }
        }
        #endregion
        public string LastPlayedString { get; set; }
        public string TotalTimeString { get; set; }
        public string Title { get; set; }
        public bool ShouldDisplayNsfwContent { get; set; }
        public bool IsMouseOver { get; set; } = false;


        public Guid UserDataId { get; set; }
        

        /// <summary>
        /// Brings up the main page of the game
        /// <see cref="MouseClick"/>
        /// </summary>
        public void MouseClick()
        {
            SetGameEntry();
            //TODO: Update this so it's global
            var vm = _container.Get<VndbContentViewModel>();
            VndbContentViewModel.SetSelectedGame(_selectedGame);
            RootViewModel.Instance.ActivateItem(vm);

        }

        /// <summary>
        /// Button for modifying settings of the game
        /// <see cref="SettingsClick"/>
        /// </summary>
        public void SettingsClick()
        {
            SetGameEntry();
            var vm = _container.Get<ModifyGameHostViewModel>();
            ModifyGameHostViewModel.SetSelectedGame(_selectedGame);
            _windowManager.ShowDialog(vm);

        }

        private void SetGameEntry()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                .Where(x => x.Id == UserDataId).FirstOrDefault();
            if (dbUserData != null)
            {
                _selectedGame = dbUserData;
            }
        }
    }
}
