using AdonisUI;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VnManager.MetadataProviders;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Dialogs.AddGameSources;
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

        public async Task ClickAsync()
        {
            var parent = _container.Get<AddGameMainViewModel>();
            var resultParent = _windowManager.ShowDialog(parent);
            if(resultParent == null)return;
            if(!resultParent.Value)return;

            //MUST add the views as singleton providers in Bootstrapper, otherwise the values will be empty
            switch (parent.SelectedSourceEnum)
            {
                case AddGameSourceType.NoSource:
                    AddNoSourceGame();
                    break;
                case AddGameSourceType.Vndb:
                    await AddVndbGameAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown Enum Source");
            }

        }

        private void AddNoSourceGame()
        {
            var vmAddGame = _container.Get<AddGameNoSourceViewModel>();
            if (vmAddGame == null) return;

            var gameEntry = new AddItemDbModel();
            gameEntry.SourceType = AddGameSourceType.NoSource;
            gameEntry.ExeType = ExeTypeEnum.Normal;
            gameEntry.IsCollectionEnabled = false;
            gameEntry.ExeCollection = null;
            gameEntry.GameId = 0;
            gameEntry.ExePath = vmAddGame.ExePath;
            gameEntry.IsIconEnabled = vmAddGame.IsIconChecked;
            gameEntry.IconPath = vmAddGame.IconPath;
            gameEntry.IsArgumentsEnabled = vmAddGame.IsArgsChecked;
            gameEntry.ExeArguments = vmAddGame.ExeArguments;
            MetadataCommon.SaveUserData(gameEntry);
        }


        private async Task AddVndbGameAsync()
        {
            var vmAddGame = _container.Get<AddGameVndbViewModel>();
            if (vmAddGame == null) return;

            var gameEntry = new AddItemDbModel();
            gameEntry.SourceType = AddGameSourceType.Vndb;
            gameEntry.ExeType = vmAddGame.ExeType;
            gameEntry.IsCollectionEnabled = vmAddGame.ExeType == ExeTypeEnum.Collection;
            gameEntry.ExeCollection = null;
            gameEntry.GameId = vmAddGame.VnId;
            gameEntry.ExePath = vmAddGame.ExePath;
            gameEntry.IsIconEnabled = vmAddGame.IsIconChecked;
            gameEntry.IconPath = vmAddGame.IconPath;
            gameEntry.IsArgumentsEnabled = vmAddGame.IsArgsChecked;
            gameEntry.ExeArguments = vmAddGame.ExeArguments;
            GetVndbData getData = new GetVndbData();
            await getData.GetDataAsync(gameEntry);
        }


    }

}
