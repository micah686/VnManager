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
        internal static Guid UserDataId { get; private set; }

        internal static int VnId { get; private set; }

        public VndbContentViewModel()
        {
            var vInfo = new VndbInfoViewModel { DisplayName = App.ResMan.GetString("Main") };
            var vChar = new VndbCharactersViewModel { DisplayName = App.ResMan.GetString("Characters") };
            var vScreen = new VndbScreensViewModel { DisplayName = App.ResMan.GetString("Screenshots") };

            Items.Add(vInfo);
            Items.Add(vChar);
            Items.Add(vScreen);
        }

        
        internal static void SetGameId(Guid guid)
        {
            UserDataId = guid;
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

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            UserDataId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        }
    }

    public class TagTraitBinding
    {
        public string Parent { get; set; }
        /// <summary>
        /// Tuple is NameOfChild, NameOfColor
        /// </summary>
        public List<Tuple<string, string>> Children { get; set; }
        public Thickness ParentMargin { get; set; }
    }
}
