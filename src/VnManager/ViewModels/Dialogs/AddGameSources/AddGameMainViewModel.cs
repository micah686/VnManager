// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Stylet;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameMainViewModel: Conductor<Screen>
    {
        public int SelectedIndex { get; set; }
        public bool CanChangeSource { get; set; }

        private readonly Func<AddGameVndbViewModel> _addVndb;
        private readonly Func<AddGameNoSourceViewModel> _addNoSource;
        private readonly IWindowManager _windowManager;
        public AddGameMainViewModel(IWindowManager windowManager, Func<AddGameVndbViewModel> vndb, Func<AddGameNoSourceViewModel> noSource)
        {
            _windowManager = windowManager;
            _addVndb = vndb;
            _addNoSource = noSource;
            CanChangeSource = true;
            SourceChanged();
            SelectedIndex = 0;
        }

        public override Task<bool> CanCloseAsync()
        {
            if (!CanChangeSource)
            {
                _windowManager.ShowMessageBox(App.ResMan.GetString("ClosingDisabledMessage"), App.ResMan.GetString("ClosingDisabledTitle"), MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return Task.FromResult(false);

            }
            return base.CanCloseAsync();
        }

        public void SourceChanged()
        {
            switch ((AddGameSourceType)SelectedIndex)
            {
                case AddGameSourceType.Vndb:
                    ActivateItem(_addVndb());
                    break;
                case AddGameSourceType.NoSource:
                    ActivateItem(_addNoSource());
                    break;
                default:
                    App.Logger.Warning("No valid source type is selected,AddGameMainViewModel");
                    break;
            }
        }

        public static UserDataGames GetDefaultUserDataEntry =>
            new UserDataGames
            {
                Id = Guid.NewGuid(),
                GameId = 0,
                LastPlayed = DateTime.MinValue,
                PlayTime = TimeSpan.Zero,
                Categories = new Collection<string>(new List<string> { "All" })
            };
    }
    public enum AddGameSourceType { NoSource, Vndb }
    public enum ExeType { Normal, Launcher, Collection }
}
