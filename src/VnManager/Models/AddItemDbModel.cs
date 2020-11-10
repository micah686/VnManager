using System;
using System.Collections.Generic;
using System.Text;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.Windows;

namespace VnManager.Models
{
    public class AddItemDbModel
    {
        public AddGameSourceType SourceType { get; set; }
        public ExeType ExeType { get; set; }
        public bool IsCollectionEnabled { get; set; }
        public  List<MultiExeGamePaths> ExeCollection { get; set; }
        public int GameId { get; set; }
        public string ExePath { get; set; }
        public bool IsIconEnabled { get; set; }
        public string IconPath { get; set; }
        public bool IsArgumentsEnabled { get; set; }
        public string ExeArguments { get; set; }

        /// <summary>
        /// CoverPath should ONLY be used for NoSource games
        /// </summary>
        public string CoverPath { get; set; }
        /// <summary>
        /// Title should ONLY be used for NoSource games
        /// </summary>
        public string Title { get; set; }
    }
}
