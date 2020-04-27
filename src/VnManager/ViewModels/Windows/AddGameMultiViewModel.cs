using Stylet;
using System;
using System.Collections.Generic;
using System.Text;

namespace VnManager.ViewModels.Windows
{
    public class AddGameMultiViewModel: Screen
    {
        public BindableCollection<MultiExeGamePaths> GameCollection { get; set; } = new BindableCollection<MultiExeGamePaths>();
        
        
        private readonly IWindowManager _windowManager;
        public AddGameMultiViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            for (int i = 0; i < 25; i++)
            {
                GameCollection.Add(new MultiExeGamePaths { ExePath = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis p", IconPath = "testico", ArgumentsString = "testarg" });
            }
            
            
        }

        public class MultiExeGamePaths
        {
            public string ExePath { get; set; }
            public string IconPath { get; set; }
            public string ArgumentsString { get; set; }
        }
    }
}
