using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Stylet;
using VnManager.Events;
using VnManager.MetadataProviders.Vndb;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameRepairViewModel: Screen
    {

        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        public ModifyGameRepairViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            DisplayName = App.ResMan.GetString("RepairUpdate");
            _windowManager = windowManager;
            _events = events;

        }

        /// <summary>
        /// Command to repair data, referenced by the View
        /// <see cref="RepairData"/>
        /// </summary>
        /// <returns></returns>
        public async Task RepairData()
        {

            var source = ModifyGameHostViewModel.SelectedGame.SourceType;
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
            var result =  _windowManager.ShowMessageBox(
                $"{App.ResMan.GetString("RepairMessage1")}\n{App.ResMan.GetString("RepairMessage2")}",
                App.ResMan.GetString("RepairVndb"), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var parentHost = (ModifyGameHostViewModel)Parent;
                parentHost.LockControls();
                
                ModifyGameDeleteViewModel.DeleteVndbContent();
                GetVndbData getData = new GetVndbData();
                await getData.GetDataAsync(ModifyGameHostViewModel.SelectedGame.GameId.Value);
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
