﻿using System;
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
            if (CredentialManager.GetCredentials("VnManager.DbEnc") == null)
            {
                File.Delete(Path.Combine(App.ConfigDirPath, @"config\config.json"));
                Environment.Exit(0);
            }
        }

        public async Task CreatePasswordClick()
        {
            
            try
            {
                if (RequirePasswordChecked)
                {

                    var hashStruct = Secure.GenerateHash(ConfirmPassword, Secure.GenerateRandomSalt());
                    string username = $"{hashStruct.Hash}|{hashStruct.Salt}";
                    
                    var cred = new NetworkCredential(username, ConfirmPassword);
                    CredentialManager.SaveCredentials("VnManager.DbEnc", cred);

                    using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={cred.Password}") { };

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
                    foreach (var character in "123456") //TODO:note-use 123456 for easy password when debugging the database
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
                    CredentialManager.SaveCredentials("VnManager.DbEnc", cred);

                    using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={cred.Password}") { };

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

            if (CredentialManager.GetCredentials("VnManager.FileEnc") == null)
            {
                var cred = new NetworkCredential("", Secure.GenerateSecurePassword(64,16));
                CredentialManager.SaveCredentials("VnManager.FileEnc", cred);
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
            var cred = CredentialManager.GetCredentials("VnManager.DbEnc");
            if (cred == null|| cred.UserName.Length <1) return false;
            var split = cred.UserName.Split('|');
            var hashSalt = new  Secure.PassHashStruct()
            {
                Hash = split[0], Salt = split[1]
            };
            bool result = Secure.ValidatePassword(instance.Password, hashSalt.Salt, hashSalt.Hash);
            return result;
        }

        

        private bool IsNoDbError(SecureString password)
        {
            try
            {
                var cred = CredentialManager.GetCredentials("VnManager.DbEnc");
                if (cred == null || cred.UserName.Length < 1) return false;
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={cred.Password}");
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

        private string CreateDbErrorMessage(SetEnterPasswordViewModel instance)
        {
            try
            {
                var cred = CredentialManager.GetCredentials("VnManager.DbEnc");
                if (cred == null || cred.UserName.Length < 1) return App.ResMan.GetString("PasswordNoEmpty");
                using var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password={cred.Password}");
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
