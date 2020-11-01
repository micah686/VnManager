using System.IO;
using System.Windows;
using System.Windows.Threading;
using FluentValidation;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Converters;
using VnManager.Utilities;
using VnManager.ViewModels;
using MvvmDialogs;
using VnManager.Helpers;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Windows;
using VnManager.ViewModels.Controls;

namespace VnManager
{
    public class Bootstrapper: Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            // This is called just after the application is started, but before the IoC container is set up.
            // Set up things like logging, etc
            
            Initializers.Startup.SetDirectories();
            Initializers.Startup.DeleteOldLogs();
            Initializers.Startup.DeleteOldBackupDatabase();
            LogManager.UpdateLoggerDirectory();
            App.StartupLockout = true; //lock any App SetOnce settings from being set again
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);
            // Bind your own types. Concrete types are automatically self-bound.
            //builder.Bind<IMyInterface>().To<MyType>();
            //builder.Bind<IViewModelFactory>().ToAbstractFactory();
            
            builder.Bind(typeof(IModelValidator<>)).To(typeof(FluentModelValidator<>));
            builder.Bind(typeof(IValidator<>)).ToAllImplementations();
            builder.Bind(typeof(IDialogService)).To(typeof(DialogService));

            builder.Bind<StatusBarViewModel>().ToSelf().InSingletonScope();
            builder.Bind<IMessageBoxViewModel>().To<CustomMsgBoxViewModel>();
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
            App.Logger.Fatal(e.Exception, "Program Crashed!");
            AdonisUI.Controls.MessageBox.Show($"Program Crashed!", "Program Crashed!", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Error);
            // Called on Application.DispatcherUnhandledException
        }
    }
}
