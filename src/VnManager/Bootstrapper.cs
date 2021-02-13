// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Windows;
using System.Windows.Threading;
using FluentValidation;
using Stylet;
using StyletIoC;
using VnManager.Utilities;
using VnManager.ViewModels;
using MvvmDialogs;
using Sentry;
using VnManager.ViewModels.Controls;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.UserControls.MainPage.NoSource;
using VnManager.ViewModels.UserControls.MainPage.Vndb;


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

            builder.Bind<IMessageBoxViewModel>().To<CustomMsgBoxViewModel>();

            builder.Bind<NavigationController>().And<INavigationController>().To<NavigationController>().InSingletonScope();
            
            //NOTE: prevents Stack Overflow exception on navigationController
            builder.Bind<Func<VndbContentViewModel>>().ToFactory<Func<VndbContentViewModel>>(c => () => c.Get<VndbContentViewModel>());
            builder.Bind<Func<NoSourceMainViewModel>>().ToFactory<Func<NoSourceMainViewModel>>(c => () => c.Get<NoSourceMainViewModel>());
            builder.Bind<Func<GameCardViewModel>>().ToFactory<Func<GameCardViewModel>>(c => () => c.Get<GameCardViewModel>());
            builder.Bind<Func<AddGameMainViewModel>>().ToFactory<Func<AddGameMainViewModel>>(c => () => c.Get<AddGameMainViewModel>());

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
            var navigationController = this.Container.Get<NavigationController>();
            navigationController.Delegate = this.RootViewModel;
            navigationController.NavigateToMainGrid();
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
