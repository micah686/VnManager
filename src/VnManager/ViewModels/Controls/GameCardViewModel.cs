// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.ModifyGame;
using VnManager.ViewModels.UserControls.MainPage.Vndb;


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
                if (value == null)
                {
                    return;
                }
                if (value.IsNsfw == false || ShouldDisplayNsfwContent)
                {
                    _coverImage = value;
                    SetAndNotify(ref _coverImage, value);
                }
                else
                {
                    const int blurWeight = 10;
                    value.Image = ImageHelper.BlurImage(value.Image, blurWeight);
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
            var modifyHost = _container.Get<Func<ModifyGameHostViewModel>>().Invoke();
            modifyHost.SetSelectedGame(_selectedGame);
            _windowManager.ShowDialog(modifyHost);
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
