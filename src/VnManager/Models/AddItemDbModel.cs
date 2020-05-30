using System;
using System.Collections.Generic;
using System.Text;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Windows;

namespace VnManager.Models
{
    public class AddItemDbModel
    {
        public AddGameSourceTypes SourceType { get; set; }
        public ExeTypesEnum ExeType { get; set; }
        public bool IsCollectionEnabled { get; set; }
        public List<MultiExeGamePaths> ExeCollection { get; set; }
        public int GameId { get; set; }
        public string ExePath { get; set; }
        public bool IsIconEnabled { get; set; }
        public string IconPath { get; set; }
        public bool IsArgumentsEnabled { get; set; }
        public string ExeArguments { get; set; }
    }
}
