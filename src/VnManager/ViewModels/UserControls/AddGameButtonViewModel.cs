using AdonisUI;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VnManager.MetadataProviders.NoSource;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Windows;

namespace VnManager.ViewModels.UserControls
{
    public class AddGameButtonViewModel: Conductor<Screen>
    {
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;
        public AddGameButtonViewModel(IContainer container, IWindowManager windowManager) 
        {
            _container = container;
            _windowManager = windowManager;
        }

        public async Task Click()
        {
            //var inst = MainGridViewModel.Instance;
            //inst.Click();


            var vmAddGame = _container.Get<AddGameViewModel>();

            var dialog = _windowManager.ShowDialog(vmAddGame);
            if (dialog == null) return;
            var result = dialog.Value;
            if (!result) return;

            var gameEntry = new AddItemDbModel();
            gameEntry.SourceType = vmAddGame.SourceTypes;
            gameEntry.ExeType = vmAddGame.ExeTypes;
            gameEntry.IsCollectionEnabled = gameEntry.ExeType == ExeTypesEnum.Collection;
            gameEntry.ExeCollection = vmAddGame.ExeCollection;
            gameEntry.GameId = vmAddGame.VnId;
            gameEntry.ExePath = vmAddGame.ExePath;
            gameEntry.IsIconEnabled = vmAddGame.IsIconChecked;
            gameEntry.IconPath = vmAddGame.IconPath;
            gameEntry.IsArgumentsEnabled = vmAddGame.IsArgsChecked;
            gameEntry.ExeArguments = vmAddGame.ExeArguments;
            

            switch (gameEntry.SourceType)
            {
                case AddGameSourceTypes.NoSource:
                    var saveData = new SaveNoSourceGameData();
                    saveData.SaveUserData(gameEntry);
                    break;
                case AddGameSourceTypes.Vndb:
                    GetVndbData getData = new GetVndbData();
                    await getData.GetData(gameEntry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
            
        }
    }

}
