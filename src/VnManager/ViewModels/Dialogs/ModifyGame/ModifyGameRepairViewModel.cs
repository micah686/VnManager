// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using Sentry;
using Stylet;
using VnManager.Events;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Dialogs.AddGameSources;

namespace VnManager.ViewModels.Dialogs.ModifyGame
{
    public class ModifyGameRepairViewModel: Screen
    {
        public bool VndbEnabled { get; set; }
        public bool NoSourceEnabled { get; set; }
        public int SelectedIndex { get; set; } = -1;
        
        internal UserDataGames SelectedGame;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly Func<ModifyGameDeleteViewModel> _gameDelete;
        public ModifyGameRepairViewModel(IWindowManager windowManager, IEventAggregator events, Func<ModifyGameDeleteViewModel> gameDelete)
        {
            DisplayName = App.ResMan.GetString("RepairUpdate");
            _windowManager = windowManager;
            _gameDelete = gameDelete;
            _events = events;
            
        }

        protected override void OnViewLoaded()
        {
            SetVisibility();
        }

        /// <summary>
        /// Set Visibility of Repair Tabs
        /// </summary>
        private void SetVisibility()
        {
            var source = SelectedGame.SourceType;
            if (source == AddGameSourceType.Vndb)
            {
                VndbEnabled = true;
                NoSourceEnabled = false;
                SelectedIndex = 0;
            }

            if (source == AddGameSourceType.NoSource)
            {
                VndbEnabled = false;
                NoSourceEnabled = true;
                SelectedIndex = 1;
            }
        }
        
        /// <summary>
        /// Command to repair data, referenced by the View
        /// <see cref="RepairData"/>
        /// </summary>
        /// <returns></returns>
        public async Task RepairData()
        {

            var source = SelectedGame.SourceType;
            if (source == AddGameSourceType.Vndb)
            {
                await RepairVndbData();
            }
        }

        /// <summary>
        /// Try repairing VndbData
        /// </summary>
        /// <returns></returns>
        public async Task RepairVndbData()
        {
            try
            {
                var result = _windowManager.ShowMessageBox(
                    $"{App.ResMan.GetString("RepairMessage1")}\n{App.ResMan.GetString("RepairMessage2")}",
                    App.ResMan.GetString("RepairVndb"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var parentHost = (ModifyGameHostViewModel)Parent;
                    parentHost.LockControls();

                    var modifyDelete = _gameDelete();
                    modifyDelete.SetSelectedGame(SelectedGame);
                    modifyDelete.DeleteVndbContent();
                    GetVndbData getData = new GetVndbData();
                    await getData.GetDataAsync(SelectedGame.GameId.Value, true);
                    _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
                    parentHost.UnlockControls();
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to repair Vndb Data");
                SentrySdk.CaptureException(e);
            }
        }
    }
}
