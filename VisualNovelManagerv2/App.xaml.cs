using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VisualNovelManagerv2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
			//TODO:Perhaps use an ICommand, and make these private?
            StartupValidate sv = new StartupValidate();
            sv.CreateFolders();
            sv.CheckForDatabase();
            sv.CheckXmlConfig();
            sv.LoadXmlSettings();
        }
    }

}

