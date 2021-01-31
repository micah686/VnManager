// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;
using FluentValidation;
using Stylet;
using StyletIoC;
using VnManager.Utilities;
using VnManager.ViewModels;
using MvvmDialogs;
using Sentry;
using Sentry.Protocol;
using VnManager.Interfaces;
using VnManager.ViewModels.UserControls;
using VnManager.ViewModels.Controls;


namespace VnManager
{
    public class Bootstrapper: Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            // This is called just after the application is started, but before the IoC container is set up.
            // Set up things like logging, etc
            Initializers.PreStartupCheck.StartupPrep();

            LogManager.UpdateLoggerDirectory();
            App.StartupLockout = true; //lock any App SetOnce settings from being set again
        }

        /// <summary>
        /// Bind your own types. Concrete types are automatically self-bound.
        /// builder.Bind<IMyInterface>().To<MyType>();
        /// builder.Bind<IViewModelFactory>().ToAbstractFactory();
        /// </summary>
        /// <param name="builder"></param>
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);
            if (builder == null)
            {
                return;
            }
            
            builder.Bind(typeof(IModelValidator<>)).To(typeof(FluentModelValidator<>));
            builder.Bind(typeof(IValidator<>)).ToAllImplementations();
            builder.Bind(typeof(IDialogService)).To(typeof(DialogService));

            builder.Bind<StatusBarViewModel>().ToSelf().InSingletonScope();
            builder.Bind<IMessageBoxViewModel>().To<CustomMsgBoxViewModel>();
            
            //Abstract Factories
            builder.Bind<IMainGridFactory>().ToAbstractFactory();
            builder.Bind<ISettingsFactory>().ToAbstractFactory();
            builder.Bind<IDebugFactory>().ToAbstractFactory();
            builder.Bind<IModifyGamePathFactory>().ToAbstractFactory();
            builder.Bind<IModifyGameCategoriesFactory>().ToAbstractFactory();
            builder.Bind<IModifyGameDeleteFactory>().ToAbstractFactory();
            builder.Bind<IModifyGameRepairFactory>().ToAbstractFactory();
            builder.Bind<IModifyGameHostFactory>().ToAbstractFactory();
            builder.Bind<IGameCardFactory>().ToAbstractFactory();
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
            if (e == null)
            {
                return;
            }
            //always want a verbose log if the program crashes
            LogManager.SetLogLevel(LogLevel.Verbose);
            App.Logger.Fatal(e.Exception, "Program Crashed!");
            SentrySdk.CaptureException(e.Exception);
            SentrySdk.CaptureMessage("Program Crashed", SentryLevel.Fatal);
            AdonisUI.Controls.MessageBox.Show($"Program Crashed!", "Program Crashed!", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Error);
            // Called on Application.DispatcherUnhandledException
        }
    }
}
