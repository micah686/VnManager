using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentValidation;
using LiteDB;
using NeoSmart.SecureStore;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Settings;
using VnManager.ViewModels.UserControls;

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
            ValidateFiles();
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            Title = rm.GetString("CreatePassTitle");
            if (!App.UserSettings.EncryptionEnabled) return;
            IsCreatePasswordVisible = false; //sets form to unlock mode
            Title = rm.GetString("UnlockDbTitle");
            if ((!File.Exists(Path.Combine(App.ConfigDirPath, @"secure\secrets.store")) || 
                 !File.Exists(Path.Combine(Path.Combine(App.ConfigDirPath, @"secure\secrets.key")))))
            {
                File.Delete(Path.Combine(App.ConfigDirPath, @"config\config.json"));
                Environment.Exit(0);
            }
            if (new Secure().TestSecret("ConnStr") == false)
            {
                File.Delete(Path.Combine(App.ConfigDirPath, @"database\Data.db"));
                Environment.Exit(0);
            }
        }

        public void CreatePasswordClick()
        {
            
            try
            {
                if (RequirePasswordChecked)
                {
                    var enc = new Secure();
                    enc.CreateSecureStore();

                    enc.SetSecret("ConnStr", $"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(Password))}");
                    using (var db = new LiteDatabase(enc.ReadSecret("ConnStr"))) { }
                    var settings = new UserSettings
                    {
                        EncryptionEnabled = true,
                        IsVisibleSavedNsfwContent = false
                    };
                    UserSettingsHelper.SaveUserSettings(settings);
                    RequestClose(true);
                }
                else
                {
                    var enc = new Secure();
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
            IsPasswordCheckClicked = true;
           // var validator = new SetEnterPasswordViewModelValidator();
           // await validator.ValidateAsync(this);
            bool result = await ValidateAsync();
            if (result)
            {
                RequestClose(true);
            }
            _attemptCounter += 1;
            await PasswordAttemptChecker();
            IsPasswordCheckClicked = false;
        }

        public async Task UnlockPasswordKeyPressed(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await UnlockPasswordClick();
            }
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


        private void ValidateFiles()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.json");
            var secretStore = Path.Combine(App.ConfigDirPath, @"secure\secrets.store");
            var secretKey = Path.Combine(App.ConfigDirPath, @"secure\secrets.key");
            var database = Path.Combine(App.ConfigDirPath, @"database\Data.db");

            if (!File.Exists(configFile))
            {
                UserSettingsHelper.CreateDefaultConfig();
            }

            if (UserSettingsHelper.ValidateConfigFile()== false)
            {
                File.Delete(configFile);
                UserSettingsHelper.CreateDefaultConfig();
            }

            if (App.UserSettings.EncryptionEnabled == true && !File.Exists(database))
            {
                File.Delete(configFile);
                App.UserSettings = UserSettingsHelper.ReadUserSettings();
            }

            if (!File.Exists(secretStore) || !File.Exists(Path.Combine(secretKey)))
            {
                Directory.Delete(Path.Combine(App.ConfigDirPath, @"secure"), true);
                Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"secure"));
                return;
            }

            string[] secKeys = new[] { "FileEnc", "ConnStr" };
            var encSt = new Secure();
            if (secKeys.Select(key => encSt.TestSecret(key)).Any(output => output == false))
            {
                Directory.Delete(Path.Combine(App.ConfigDirPath, @"secure"), true);
                Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"secure"));
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
                RuleFor(x => x.Password).Cascade(CascadeMode.StopOnFirstFailure)
                    .Must(IsNoDbError).WithMessage(CreateDbErrorMessage);

            });
        }



        private bool DoPasswordsMatch(SetEnterPasswordViewModel instance, SecureString confirmPass)
        {
            return Secure.SecureStringEqual(instance.Password, confirmPass);
        }

        

        private bool IsNoDbError(SecureString password)
        {
            try
            {
                if (password == null) return false;
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(password))}");
                return true;
            }
            catch (IOException ex)
            {
                return false;
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

        private string CreateDbErrorMessage(SetEnterPasswordViewModel instance)
        {
            try
            {
                if (instance.Password == null) return App.ResMan.GetString("PasswordNoEmpty");
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(instance.Password))}");
                return String.Empty;
            }
            catch (IOException ex)
            {
                return App.ResMan.GetString("DbIsLockedProc");
            }
            catch (LiteException ex)
            {
                return ex.Message == "Invalid password" ? App.ResMan.GetString("PassIncorrect") : ex.Message;
            }
            catch (Exception ex)
            {
                App.Logger.Error("CreateDbErrorMessage an unknown error occurred");
                return App.ResMan.GetString("UnknownException");
            }
        }
        
    }



}
