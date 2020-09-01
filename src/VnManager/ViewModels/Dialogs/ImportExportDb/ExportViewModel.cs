using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdysTech.CredentialManager;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using Stylet;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs.ImportExportDb
{
    public class ExportViewModel: Screen
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;

        public ExportViewModel(IWindowManager windowManager, IDialogService dialogService)
        {
            _windowManager = windowManager;
            _dialogService = dialogService;
        }

        public void ExportData()
        {

            string savePath;
            var settings = new FolderBrowserDialogSettings();
            bool? result = _dialogService.ShowFolderBrowserDialog(this, settings);
            if (result == true)
            {
                savePath = settings.SelectedPath;
            }
            else
            {
                return;
            }
            var fileName = $@"{savePath}\VnManager_Export_{DateTime.UtcNow:yyyy-MMMM-dd}.db";
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").FindAll();

                using (var exportDatabase = new LiteDatabase(fileName))
                {
                    var exportUserData = exportDatabase.GetCollection<UserDataGames>("UserData_Games");

                    List<UserDataGames> userDataList = dbUserData.Select(item => new UserDataGames
                    {
                        Id = item.Id,
                        GameId = item.GameId,
                        GameName = item.GameName,
                        SourceType = item.SourceType,
                        LastPlayed = item.LastPlayed,
                        PlayTime = item.PlayTime,
                        Categories = item.Categories,
                        ExePath = item.ExePath,
                        IconPath = item.IconPath,
                        Arguments = item.Arguments
                    })
                        .ToList();

                    exportUserData.Insert(userDataList);
                    _windowManager.ShowMessageBox($"{App.ResMan.GetString("UserDataExportedPath")}\n{fileName}", $"{App.ResMan.GetString("UserDataExportedTitle")}");

                }


            }
        }
    }
}
