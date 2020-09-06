using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Settings;
using VnManager.ViewModels.UserControls;

namespace VnManager.ViewModels.Windows
{
    public class SetEnterPasswordViewModel: Screen
    {
        public string Title { get; set; }
        /// <summary>
        /// Yes has been checked for creating a new password
        /// </summary>
        public bool RequirePasswordChecked { get; set; } //yes checked for creating a new password
        public SecureString Password { get; set; }
        public SecureString ConfirmPassword { get; set; }
        /// <summary>
        /// If true, shows the dialog to unlock the database
        /// If false, shows the create password dialog
        /// </summary>
        public bool IsUnlockPasswordVisible { get; set; }

        private bool _isCreatePasswordVisible = true;
        /// <summary>
        /// If true, shows the dialog to create a password
        /// If false, shows the unlock database dialog
        /// </summary>
        public bool IsCreatePasswordVisible
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
        internal bool IsClickChecked = false;
        private int _attemptCounter = 0;


        public SetEnterPasswordViewModel(IModelValidator<SetEnterPasswordViewModel> validator) : base(validator)
        {
            ValidateFiles();
            Title = App.ResMan.GetString("CreatePassTitle");
            if (App.UserSettings.EncryptionEnabled)
            {
                IsCreatePasswordVisible = false; //sets form to unlock mode
                Title = App.ResMan.GetString("UnlockDbTitle");
                if (CredentialManager.GetCredentials(App.CredDb) == null)
                {
                    File.Delete(Path.Combine(App.ConfigDirPath, @"config\config.json"));
                    Environment.Exit(0);
                }
            }
            
        }

        /// <summary>
        /// If the user has checked enable custom password, create a database with the custom password
        /// If the user is not generating a custom password, create a database with a 64 character password with 16 special characters
        /// Note: The password for Debug is 123456, in order to assist with debugging.
        /// </summary>
        /// <returns></returns>
        public async Task CreatePasswordClickAsync()
        {
            File.Delete(Path.Combine(App.ConfigDirPath, App.DbPath));
            IsClickChecked = true;
            try
            {
                if (RequirePasswordChecked)
                {
                    if (Password == null || Password.Length < 1 || ConfirmPassword == null || ConfirmPassword.Length <1)
                    {
                        await ValidateAsync();
                        return;
                    }

                    
                    var hashStruct = Secure.GenerateHash(ConfirmPassword, Secure.GenerateRandomSalt());
                    string username = $"{hashStruct.Hash}|{hashStruct.Salt}";
                    
                    var cred = new NetworkCredential(username, ConfirmPassword);
                    CredentialManager.SaveCredentials(App.CredDb, cred);
                    var validPasswords = await ValidateAsync();
                    if(validPasswords != true) return;
                    using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}")
                    {
                        //intentionally blank. Creates initial database
                    };

                    var settings = new UserSettings
                    {
                        EncryptionEnabled = true,
                        IsVisibleSavedNsfwContent = false
                    };
                    UserSettingsHelper.SaveUserSettings(settings);
                    bool result = await ValidateAsync();
                    if (result)
                    {
                        RequestClose(true);
                    }
                }
                else
                {
                    
                    var password = new SecureString();

#if DEBUG
                    foreach (var character in "123456") //TODO:Debug only password
                    {
                        password.AppendChar(character);
                    }
#else
                    foreach (var character in Secure.GenerateSecurePassword(64,16))
                    {
                        password.AppendChar(character);
                    }
#endif
                    var hashStruct = Secure.GenerateHash(password, Secure.GenerateRandomSalt());
                    string username = $"{hashStruct.Hash}|{hashStruct.Salt}";

                    var cred = new NetworkCredential(username, password);
                    CredentialManager.SaveCredentials(App.CredDb, cred);

                    using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}") { };

                    bool result = await ValidateAsync();
                    if (result)
                    {
                        RequestClose(true);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't Create Password");
                IsClickChecked = false;
                RequestClose(false);
            }
        }

        /// <summary>
        /// Attempts to unlock the database with the entered password.
        /// If the password is incorrect, it will create a validation error on the TextBox
        /// </summary>
        /// <returns></returns>
        public async Task UnlockPasswordClickAsync()
        {
            IsPasswordCheckClicked = true;
            IsClickChecked = true;
            bool result = await ValidateAsync();
            if (result)
            {
                RequestClose(true);
            }
            _attemptCounter += 1;
            await PasswordAttemptCheckerAsync();
            IsPasswordCheckClicked = false;
            IsClickChecked = false;
        }

        /// <summary>
        /// Tries to unlock the database if [Enter] is pressed
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task UnlockPasswordKeyPressedAsync(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await UnlockPasswordClickAsync();
            }
        }

        /// <summary>
        /// Checks how many password attempts were made
        /// If too many attempts are made, the time between attempts is increased
        /// If there have been 50 attempts, the program exits
        /// </summary>
        /// <returns></returns>
        private async Task PasswordAttemptCheckerAsync()
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
            else if (_attemptCounter >= 50)
            {
                Environment.Exit(0);
            }
            else 
            {
                return;
            }
        }

        /// <summary>
        /// Validates the settings of the config file and the database
        /// </summary>
        private void ValidateFiles()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.json");
            var database = Path.Combine(App.ConfigDirPath, App.DbPath);

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

            if (CredentialManager.GetCredentials(App.CredFile) == null)
            {
                var cred = new NetworkCredential(Convert.ToBase64String(Secure.GenerateRandomSalt()), Secure.GenerateSecurePassword(64,16));
                CredentialManager.SaveCredentials(App.CredFile, cred);
            }



        }

    }

    public class SetEnterPasswordViewModelValidator: AbstractValidator<SetEnterPasswordViewModel>
    {
        public SetEnterPasswordViewModelValidator()
        {
            When(x => x.IsCreatePasswordVisible && x.RequirePasswordChecked && x.IsClickChecked, () =>
            {
                RuleFor(x => x.Password).NotNull().WithMessage(App.ResMan.GetString("PasswordNoEmpty"));

                RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.StopOnFirstFailure)
                    .NotNull().WithMessage(App.ResMan.GetString("PasswordNoEmpty"))
                    .Must(DoPasswordsMatch).WithMessage(App.ResMan.GetString("PasswordsNoMatch"));

            });

            When(x => x.IsUnlockPasswordVisible && x.IsPasswordCheckClicked && x.IsClickChecked, () =>
            {
                RuleFor(x => x.Password).Cascade(CascadeMode.StopOnFirstFailure)
                    .Must(IsNoDbError).WithMessage(CreateDbErrorMessage)
                    .Must(DoPasswordsMatch).WithMessage(App.ResMan.GetString("PasswordsNoMatch"));

            });
        }


        /// <summary>
        /// Checks if first password and the confirmation password match
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="confirmPass"></param>
        /// <returns></returns>
        //TODO:try to get rid of instance, if I can
        private bool DoPasswordsMatch(SetEnterPasswordViewModel instance, SecureString confirmPass)
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null|| cred.UserName.Length <1) return false;
            var split = cred.UserName.Split('|');
            var hashSalt = new  Secure.PassHash()
            {
                Hash = split[0], Salt = split[1]
            };
            bool result = Secure.ValidatePassword(instance.Password, hashSalt.Salt, hashSalt.Hash);
            return result;
        }

        
        /// <summary>
        /// Checks if trying to open the database generates an error
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool IsNoDbError(SecureString password)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return false;
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}");
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (LiteException)
            {
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex,"DidEnterWithRightPassword an unknown error occurred");
                return false;
            }
        }

        /// <summary>
        /// Tries to create the database, and generates a validation error if it fails
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private string CreateDbErrorMessage(SetEnterPasswordViewModel instance)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return App.ResMan.GetString("PasswordNoEmpty");
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}");
                return String.Empty;
            }
            catch (IOException)
            {
                return App.ResMan.GetString("DbIsLockedProc");
            }
            catch (LiteException ex)
            {
                return ex.Message == "Invalid password" ? App.ResMan.GetString("PassIncorrect") : ex.Message;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex,"CreateDbErrorMessage an unknown error occurred");
                return App.ResMan.GetString("UnknownException");
            }
        }
        
    }



}
