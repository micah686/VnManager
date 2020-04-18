using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Serilog;
using Stylet;
using StyletIoC;
using VnManager.Converters;
using VnManager.ViewModels;

namespace VnManager
{
    public class Bootstrapper: Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            // This is called just after the application is started, but before the IoC container is set up.
            // Set up things like logging, etc
            Initializers.Startup.CreatFolders();
            Globals.Logger = new LoggerConfiguration().WriteTo.File(new SerilogFormatter(), string.Format(@"{0}\Data\logs\{1}-{2}-{3}_{4}.log", Globals.DirectoryPath, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, Globals.Loglevel.ToString())).CreateLogger();
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Bind your own types. Concrete types are automatically self-bound.
            //builder.Bind<IMyInterface>().To<MyType>();
        }

        protected override void Configure()
        {
            // This is called after Stylet has created the IoC container, so this.Container exists, but before the
            // Root ViewModel is launched.
            // Configure your services, etc, in here
        }

        protected override void OnLaunch()
        {
            // This is called just after the root ViewModel has been launched
            // Something like a version check that displays a dialog might be launched from here
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Called on Application.Exit
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            // Called on Application.DispatcherUnhandledException
        }
    }
}
