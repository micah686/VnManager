using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbContentViewModel: Conductor<Screen>.Collection.OneActive
    {

        internal static int VnId { get; private set; }

        internal static UserDataGames SelectedGame { get; private set; }

        public VndbContentViewModel()
        {
            var vInfo = new VndbInfoViewModel { DisplayName = App.ResMan.GetString("Main") };
            var vChar = new VndbCharactersViewModel { DisplayName = App.ResMan.GetString("Characters") };
            var vScreen = new VndbScreensViewModel { DisplayName = App.ResMan.GetString("Screenshots") };

            Items.Add(vInfo);
            Items.Add(vChar);
            Items.Add(vScreen);
        }


        internal static void SetSelectedGame(UserDataGames game)
        {
            SelectedGame = game;
            VnId = SelectedGame.GameId;
        }
        

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            SelectedGame = new UserDataGames();
        }
    }

    
}
