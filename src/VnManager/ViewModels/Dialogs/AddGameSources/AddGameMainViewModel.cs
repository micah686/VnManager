using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameMainViewModel: Conductor<Screen>
    {
        public enum AddGameSourceType { NotSet, NoSource, Vndb }
        public enum ExeTypeEnum { Normal, Launcher, Collection }
        //public AddGameSourceType SourceType { get; set; } = AddGameSourceType.Vndb;

        public int SelectedIndex { get; set; }

        public IEnumerable<string> SourceCollection { get; set; } = new[] {"Vndb", "No Source"};
        public string SelectedSource { get; set; }
        public AddGameSourceType SelectedSourceEnum = AddGameSourceType.NotSet;
        public bool CanChangeSource { get; set; }


        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public AddGameMainViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
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
                    break;
            }
        }
    }
}
