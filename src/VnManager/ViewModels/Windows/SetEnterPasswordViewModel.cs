using System;
using System.Collections.Generic;
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
        public string Title { get; set; }
        public bool IsCreatePassword { get; set; } = true;
        public bool RequirePasswordChecked { get; set; }
        public SecureString Password { get; set; }
        public SecureString ConfirmPassword { get; set; }
        public string Test { get; set; }

        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public SetEnterPasswordViewModel(IContainer container, IWindowManager windowManager, IModelValidator<SetEnterPasswordViewModel> validator): base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            Title = "Set a password";
            //if (File.Exists(Path.Combine(App.ConfigDirPath, @"secure\secrets.store")))
            //{
            //    IsCreatePassword = false;
            //}
        }



        public void SetPassword()
        {
            var validator = new SetEnterPasswordViewModelValidator();
            

            //if (RequirePasswordChecked)
            //{
            //    var result = EncryptedStore.SecureStringEqual(Password, ConfirmPassword);
            //}


        }

    }



    public class SetEnterPasswordViewModelValidator : AbstractValidator<SetEnterPasswordViewModel>
    {
        public SetEnterPasswordViewModelValidator()
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage("can't be empty");
        }
    }





}
