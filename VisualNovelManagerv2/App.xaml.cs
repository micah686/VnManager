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
            StartupValidate sv = new StartupValidate();
            sv.CreateFoldersCommand.Execute(null);
            sv.CheckForDatabaseCommand.Execute(null);
            sv.CheckXmlConfigCommand.Execute(null);
            sv.LoadXmlSettingsCommand.Execute(null);
        }
    }

}

