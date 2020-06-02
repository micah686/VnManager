using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using FluentValidation;
using Stylet;
using StyletIoC;
using VnManager.Helpers;

namespace VnManager.ViewModels.Windows
{
    public class SetEnterPasswordViewModel: Screen
    {
        #region Previous
        public string Title { get; set; }
        public bool IsCreatePassword { get; set; } = true;
        public bool RequirePasswordChecked { get; set; }
        public SecureString Password { get; set; }
        public SecureString ConfirmPassword { get; set; }
        public string Test { get; set; }

        //private readonly IWindowManager _windowManager;
        //private readonly IContainer _container;
        //public SetEnterPasswordViewModel(IContainer container, IWindowManager windowManager, IModelValidator<SetEnterPasswordViewModel> validator) : base(validator)
        //{
        //    _container = container;
        //    _windowManager = windowManager;
        //    Title = "Set a password";
        //    //if (File.Exists(Path.Combine(App.ConfigDirPath, @"secure\secrets.store")))
        //    //{
        //    //    IsCreatePassword = false;
        //    var result = EncryptedStore.SecureStringEqual(Password, ConfirmPassword);
        //    //}
        //}
        #endregion


        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public SetEnterPasswordViewModel(IContainer container, IWindowManager windowManager, IModelValidator<SetEnterPasswordViewModel> validator) : base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            Title = "Set a password";

        }
        public void Click()
        {
            Debug.WriteLine("clicked");
            //var validator = new SetEnterPasswordViewModelValidator();
            Validate();

        }


        public void OpenMain()
        {
            //var vm = _container.Get<RootViewModel>();
           // _windowManager.ShowWindow(vm);
            //this.RequestClose();
        }

    }


    



    public class SetEnterPasswordViewModelValidator: AbstractValidator<SetEnterPasswordViewModel>
    {
        public SetEnterPasswordViewModelValidator()
        {
            RuleFor(x => x.Password).Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().Unless(x => x.RequirePasswordChecked).WithMessage("Password cannot be empty");

            RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().Unless(x => x.RequirePasswordChecked).WithMessage("Password cannot be empty")
            .Must(DoPasswordsMatch).Unless(x => x.RequirePasswordChecked).WithMessage("Passwords do not match!");
        }



        private bool DoPasswordsMatch(SetEnterPasswordViewModel instance, SecureString confirmPass)
        {
            return EncryptedStore.SecureStringEqual(instance.Password, confirmPass);
        }

    }



}
