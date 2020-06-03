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
        public bool RequirePasswordChecked { get; set; } //yes checked for creating a new password
        public SecureString Password { get; set; }
        public SecureString ConfirmPassword { get; set; }
        public bool IsUnlockPasswordVisible { get; set; } //enables view for entering current password

        private bool _isCreatePasswordVisible = true;
        public bool IsCreatePasswordVisible // enables view for creating a password
        {
            get => _isCreatePasswordVisible;
            set
            {
                _isCreatePasswordVisible = value;
                IsUnlockPasswordVisible = !value;
                SetAndNotify(ref _isCreatePasswordVisible, value);
            }
        }


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

            var validator = new SetEnterPasswordViewModelValidator();
            ValidateAsync();
            bool result = validator.Validate(this).IsValid;


        }



        public void CreatePasswordClick()
        {
            if (RequirePasswordChecked)
            {

            }
            else
            {
                RequestClose(true);
            }
        }

        public void UnlockPasswordClick()
        {

        }


    }


    



    public class SetEnterPasswordViewModelValidator: AbstractValidator<SetEnterPasswordViewModel>
    {
        public SetEnterPasswordViewModelValidator()
        {

            When(x => x.IsCreatePasswordVisible && x.RequirePasswordChecked, () =>
            {
                RuleFor(x => x.Password).NotNull().WithMessage("Password cannot be empty");

                RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotNull().WithMessage("Password cannot be empty")
                    .Must(DoPasswordsMatch).WithMessage("Passwords do not match");

            });
        }



        private bool DoPasswordsMatch(SetEnterPasswordViewModel instance, SecureString confirmPass)
        {
            return EncryptedStore.SecureStringEqual(instance.Password, confirmPass);
        }

    }



}
