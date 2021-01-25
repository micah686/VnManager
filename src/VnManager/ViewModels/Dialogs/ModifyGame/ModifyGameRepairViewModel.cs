using System;
using System.Threading.Tasks;
using System.Windows;
using Stylet;
using StyletIoC;
using VnManager.Events;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameRepairViewModel: Screen
    {
        private UserDataGames _selectedGame;
        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        private readonly IEventAggregator _events;
        public ModifyGameRepairViewModel(IWindowManager windowManager, IContainer container, IEventAggregator events)
        {
            DisplayName = App.ResMan.GetString("RepairUpdate");
            _windowManager = windowManager;
            _container = container;
            _events = events;

        }

        protected override void OnViewLoaded()
        {
            var parent = (ModifyGameHostViewModel) Parent;
            _selectedGame = parent.SelectedGame;
        }

        /// <summary>
        /// Command to repair data, referenced by the View
        /// <see cref="RepairData"/>
        /// </summary>
        /// <returns></returns>
        public async Task RepairData()
        {

            var source = _selectedGame.SourceType;
            if (source == AddGameSourceType.Vndb)
            {
                await RepairVndbData();
            }

            if (source == AddGameSourceType.NoSource)
            {
                RepairNoSourceData();
            }
        }

        public async Task RepairVndbData()
        {
            var result = _windowManager.ShowMessageBox(
                $"{App.ResMan.GetString("RepairMessage1")}\n{App.ResMan.GetString("RepairMessage2")}",
                App.ResMan.GetString("RepairVndb"), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var parentHost = (ModifyGameHostViewModel)Parent;
                parentHost.LockControls();

                var modifyDelete = _container.Get<Func<ModifyGameDeleteViewModel>>().Invoke();
                modifyDelete.SetSelectedGame(_selectedGame);
                modifyDelete.DeleteVndbContent();
                GetVndbData getData = new GetVndbData();
                await getData.GetDataAsync(_selectedGame.GameId.Value);
                _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
                parentHost.UnlockControls();
            }
        }

        public void RepairNoSourceData()
        {
            throw new NotImplementedException("Need to implement for NoSource");
        }
    }
}
