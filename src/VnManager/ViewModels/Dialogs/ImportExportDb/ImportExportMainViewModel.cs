using System;
using System.Collections.Generic;
using System.Text;
using Stylet;
using StyletIoC;

namespace VnManager.ViewModels.Dialogs.ImportExportDb
{
    public class ImportExportMainViewModel: Conductor<Screen>
    {
        public ExportViewModel ExportTab { get; set; }
        public ImportViewModel ImportTab { get; set; }
        
        
        private readonly IContainer _container;

        public ImportExportMainViewModel(IContainer container)
        {
            _container = container;
            ExportTab = _container.Get<ExportViewModel>();
            ImportTab = _container.Get<ImportViewModel>();
        }
    }
}
