using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using Stylet;

namespace VnManager.ViewModels.Controls
{
    public class GameCardViewModel: Screen
    {
        public BitmapImage CoverImage { get; set; }
        public string LastPlayedString { get; set; }
        public string TotalTimeString { get; set; }
        public string Title { get; set; }
        public bool IsNsfwDisabled { get; set; }
        public bool IsMouseOver { get; set; } = false;
    }
}
