using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameHostViewModel: Conductor<Screen>.Collection.OneActive
    {
        internal static UserDataGames SelectedGame { get; private set; }
        public static string WindowTitle { get; set; }
        public static string GameTitle { get; set; }

        private readonly IContainer _container;
        public ModifyGameHostViewModel(IContainer container)
        {
            _container = container;
            var gamePath = _container.Get<ModifyGamePathViewModel>();
            var gameCategories = _container.Get<ModifyGameCategoriesViewModel>();
            var gameDelete = _container.Get<ModifyGameDeleteViewModel>();
            Items.Add(gamePath);
            Items.Add(gameCategories);
            Items.Add(gameDelete);
            
            ActivateItem(gamePath);
            
        }

        public sealed override void ActivateItem(Screen item)
        {
            base.ActivateItem(item);
        }


        internal static void SetSelectedGame(UserDataGames game)
        {
            SelectedGame = game;
            SetTitle();
            
        }

        private static void SetTitle()
        {
            switch (SelectedGame.SourceType)
            {
                case AddGameSourceType.Vndb:
                {
                    SetVndbTitle();

                    break;
                }
                case AddGameSourceType.NoSource:
                    WindowTitle =  $"{App.ResMan.GetString("Modify")} {SelectedGame.Title}";
                    GameTitle = SelectedGame.Title;
                    break;
                default:
                    //do nothing
                    break;
            }
        }

        private static void SetVndbTitle()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return;
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query()
                .Where(x => x.VnId == SelectedGame.GameId.Value).FirstOrDefault();
            if (dbUserData != null)
            {
                WindowTitle = $"{App.ResMan.GetString("Modify")} {dbUserData.Title}";
                GameTitle = dbUserData.Title;
            }
        }
    }
}
