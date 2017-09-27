using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VisualNovelManagerv2.ViewModel;
using VisualNovelManagerv2.ViewModel.Global;

namespace VisualNovelManagerv2
{
    public static class Globals
    {        
        public static readonly string DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static uint VnId;
        public static readonly StatusBarViewModel StatusBar = (new ViewModelLocator()).StatusBar;
        public static bool NsfwEnabled = false;
        public static uint MaxSpoiler = 0;
    }
}
