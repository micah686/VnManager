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
        public enum AddGameSourceType { NoSource, Vndb }
        public AddGameSourceType SourceType { get; set; } = AddGameSourceType.Vndb;

        public IEnumerable<string> SourceCollection { get; set; } = new[] {"Vndb", "No Source"};
        public string SelectedSource { get; set; }


        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public AddGameMainViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            SourceChanged();
        }

        public void SourceChanged()
        {
            switch (SelectedSource)
            {
                case "Vndb":
                    
                    break;
                case "No Source":
                    var item = _container.Get<AddGameNoSourceViewModel>();
                    ActivateItem(item);
                    break;
                default:
                    break;
            }
        }
    }
}
