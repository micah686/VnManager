using System.Windows;
using System.Windows.Threading;
using Stylet;
using StyletIoC;
using VnManager.Converters;
using VnManager.Utilities;
using VnManager.ViewModels;

namespace VnManager
{
    public class Bootstrapper: Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            // This is called just after the application is started, but before the IoC container is set up.
            // Set up things like logging, etc
            Initializers.Startup.SetDirectories();
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
            //always want a verbose log if the program crashes
            LogManager.SetLogLevel(LogLevel.Verbose);
            LogManager.Logger.Fatal(e.Exception, "Program Crashed!");
            // Called on Application.DispatcherUnhandledException
        }
    }
}
