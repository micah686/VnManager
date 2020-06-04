using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using LiteDB;
using NeoSmart.SecureStore;
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


        public bool IsPasswordCheckClicked { get; set; } = false;
        public bool IsUnlockPasswordButtonEnabled { get; set; } = true;
        private int _attemptCounter = 0;

        #endregion

        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public SetEnterPasswordViewModel(IContainer container, IWindowManager windowManager, IModelValidator<SetEnterPasswordViewModel> validator) : base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            Title = "Set a password";
            if (App.UserSettings.EncryptionEnabled)
            {
                IsCreatePasswordVisible = false;
            }
        }

        public void CreatePasswordClick()
        {
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            Title = rm.GetString("CreatePassTitle");
            try
            {
                if (RequirePasswordChecked)
                {
                    var enc = new EncryptedStore();
                    enc.CreateSecureStore();

                    enc.SetSecret("ConnStr", $"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(Password))}");
                    using (var db = new LiteDatabase(enc.ReadSecret("ConnStr"))) { }
                    RequestClose(true);
                }
                else
                {
                    var enc = new EncryptedStore();
                    enc.CreateSecureStore();
                    enc.SetSecret("ConnStr", $"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={enc.ReadSecret("FileEnc")}");
                    using (var db = new LiteDatabase(enc.ReadSecret("ConnStr"))) { }
                    RequestClose(true);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't Create Password");
                RequestClose(false);
            }
        }

        public async Task UnlockPasswordClick()
        {
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            Title = rm.GetString("UnlockDbTitle");
            IsPasswordCheckClicked = true;
            bool result = await ValidateAsync();
            if (result)
            {
                RequestClose(true);
            }
            _attemptCounter += 1;
            await PasswordAttemptChecker();
            IsPasswordCheckClicked = false;
        }



        private async Task PasswordAttemptChecker()
        {
            if(_attemptCounter <=5)
            {
                IsUnlockPasswordButtonEnabled = false;
                await Task.Delay(TimeSpan.FromMilliseconds(650));
                IsUnlockPasswordButtonEnabled = true;
            }
            if (_attemptCounter >= 5 && _attemptCounter <= 20)
            {
                IsUnlockPasswordButtonEnabled = false;
                var wait = new Random().NextDouble() * (8 - 2) + 2;
                await Task.Delay(TimeSpan.FromSeconds(wait));
                
                IsUnlockPasswordButtonEnabled = true;
            }
            if (_attemptCounter > 20 && _attemptCounter < 50)
            {
                IsUnlockPasswordButtonEnabled = false;
                await Task.Delay(TimeSpan.FromSeconds(30));
                IsUnlockPasswordButtonEnabled = true;
            }
            else if(_attemptCounter > 50)
            {
                Environment.Exit(0);
            }
        }

    }

    public class SetEnterPasswordViewModelValidator: AbstractValidator<SetEnterPasswordViewModel>
    {
        public SetEnterPasswordViewModelValidator()
        {
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            When(x => x.IsCreatePasswordVisible && x.RequirePasswordChecked, () =>
            {
                RuleFor(x => x.Password).NotNull().WithMessage(rm.GetString("PasswordNoEmpty"));

                RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotNull().WithMessage(rm.GetString("PasswordNoEmpty"))
                    .Must(DoPasswordsMatch).WithMessage(rm.GetString("PasswordsNoMatch"));

            });

            When(x => x.IsUnlockPasswordVisible && x.IsPasswordCheckClicked, () =>
            {
                RuleFor(x => x.Password).Must(DidEnterRightPassword).WithMessage(rm.GetString("PassIncorrect"));

            });
        }



        private bool DoPasswordsMatch(SetEnterPasswordViewModel instance, SecureString confirmPass)
        {
            return EncryptedStore.SecureStringEqual(instance.Password, confirmPass);
        }

        private bool DidEnterRightPassword(SecureString password)
        {
            try
            {
                if (password == null) return false;
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(password))}");
                return true;
            }
            catch (LiteException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.Error("DidEnterWithRightPassword an unknown error occurred");
                return false;
            }
        }

    }



}
