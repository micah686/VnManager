// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Stylet;
using StyletIoC;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameMainViewModel: Conductor<Screen>
    {
        

        public int SelectedIndex { get; set; }

        public IEnumerable<string> SourceCollection { get; set; } = new[] {"Vndb", "No Source"};
        public string SelectedSource { get; set; }
        internal AddGameSourceType SelectedSourceEnum = AddGameSourceType.NotSet;
        public bool CanChangeSource { get; set; }


        private readonly IContainer _container;
        public AddGameMainViewModel(IContainer container)
        {
            _container = container;
            CanChangeSource = true;
            SourceChanged();
            SelectedIndex = 0;
        }

        public void SourceChanged()
        {
            switch (SelectedSource)
            {
                case "Vndb":
                    SelectedSourceEnum = AddGameSourceType.Vndb;
                    var vndb = _container.Get<AddGameVndbViewModel>();
                    ActivateItem(vndb);
                    
                    break;
                case "No Source":
                    SelectedSourceEnum = AddGameSourceType.NoSource;
                    var noSource = _container.Get<AddGameNoSourceViewModel>();
                    ActivateItem(noSource);
                    break;
                default:
                    App.Logger.Warning("No valid source type is selected,AddGameMainViewModel");
                    break;
            }
        }

        public static UserDataGames GetDefaultUserDataEntry()
        {
            var entry = new UserDataGames
            {
                Id = Guid.NewGuid(),
                GameId = 0,
                LastPlayed = DateTime.MinValue,
                PlayTime = TimeSpan.Zero,
                Categories = new List<string> { "All" }
            };
            return entry;
        }
    }
    public enum AddGameSourceType { NotSet, NoSource, Vndb }
    public enum ExeType { Normal, Launcher, Collection }
}
