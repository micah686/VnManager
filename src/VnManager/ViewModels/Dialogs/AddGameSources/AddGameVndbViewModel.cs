using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Stylet;
using StyletIoC;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Errors;
using VndbSharp.Models.VisualNovel;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.Models.Db.User;
using VnManager.ViewModels.Windows;
using static VnManager.ViewModels.Dialogs.AddGameSources.AddGameMainViewModel;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameVndbViewModel: Screen
    {

        internal readonly List<MultiExeGamePaths> ExeCollection = new List<MultiExeGamePaths>();
        internal VndbResponse<VisualNovel> VnNameList;
        public BindableCollection<string> SuggestedNamesCollection { get; private set; }

        public bool IsLockDown { get; set; } = false;

        public int VnId { get; set; }
        public string VnName { get; set; }
        private bool _isNameChecked;
        public bool IsNameChecked
        {
            get => _isNameChecked;
            set
            {
                VnIdOrName = value == false ? App.ResMan.GetString("Id") : App.ResMan.GetString("Name");
                SetAndNotify(ref _isNameChecked, value);
            }
        }

        public string VnIdOrName { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        public bool CanChangeVnName { get; set; }
        public bool IsNameDropDownOpen { get; set; } = false;
        public int DropDownIndex { get; set; }
        public string SelectedName { get; set; }
        public bool IsSearchNameButtonEnabled { get; set; } = true;
        public bool IsResetNameButtonEnabled { get; set; } = true;
        public bool IsSearchingForNames { get; set; } = false;



        private ExeTypeEnum _exeType;
        public ExeTypeEnum ExeType
        {
            get => _exeType;
            set
            {
                SetAndNotify(ref _exeType, value);
                if (_exeType != ExeTypeEnum.Collection)
                {
                    IsNotExeCollection = true;
                }
                else
                {
                    IsNotExeCollection = false;
                }
            }
        }

        private bool _isNotExeCollection;
        public bool IsNotExeCollection
        {
            get { return _isNotExeCollection; }
            set
            {
                ExeArguments = string.Empty;
                IconPath = string.Empty;
                IsArgsChecked = false;
                IsIconChecked = false;
                SetAndNotify(ref _isNotExeCollection, value);
            }
        }


        private bool _isIconChecked;
        public bool IsIconChecked
        {
            get => _isIconChecked;
            set
            {
                SetAndNotify(ref _isIconChecked, value);
                if (_isIconChecked == false)
                {
                    IconPath = string.Empty;
                    //HideIconError = true;
                    ValidateAsync();
                    //HideIconError = false;
                }
            }
        }

        private bool _isArgsChecked;
        public bool IsArgsChecked
        {
            get => _isArgsChecked;
            set
            {
                SetAndNotify(ref _isArgsChecked, value);
                if (_isArgsChecked == false)
                {
                    ExeArguments = string.Empty;
                    //HideArgumentsError = true;
                    ValidateAsync();
                    //HideArgumentsError = false;
                }
            }
        }


        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;
        public AddGameVndbViewModel(IContainer container, IWindowManager windowManager, IModelValidator<AddGameVndbViewModel> validator, IDialogService dialogService) : base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;
            IsNotExeCollection = true;
            CanChangeVnName = true;
            SuggestedNamesCollection = new BindableCollection<string>();
        }





        public async Task SearchAsync()
        {
            if(await PreSearchCheckAsync() == false) return;

            try
            {
                using (Vndb client = new Vndb(true))
                {
                    var stopwatch = new Stopwatch();
                    bool shouldContinue = true;
                    var maxTime = TimeSpan.FromSeconds(45);
                    SuggestedNamesCollection.Clear();
                    IsSearchingForNames = true;

                    stopwatch.Start();
                    while (shouldContinue)
                    {
                        if (stopwatch.Elapsed > maxTime) return;
                        shouldContinue = false;
                        VnNameList = await client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(VnName), VndbFlags.Basic);
                        //do I need to check for null?
                        if (VnNameList.Count < 1 && client.GetLastError().Type == ErrorType.Throttled)
                        {
                            await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                            shouldContinue = true;
                        }
                        else if (VnNameList.Count < 1)
                        {
                            HandleVndbErrors.HandleErrors(client.GetLastError());
                            return;
                        }
                        else
                        {
                            List<string> nameList = IsJapaneseText(VnName) == true ? VnNameList.Select(item => item.OriginalName).ToList() : VnNameList.Select(item => item.Name).ToList();
                            SuggestedNamesCollection.AddRange(nameList.Where(x => !string.IsNullOrEmpty(x)).ToList());
                            IsNameDropDownOpen = true;
                            SelectedName = SuggestedNamesCollection.FirstOrDefault();
                        }
                    }
                }
                IsResetNameButtonEnabled = true;
                IsSearchingForNames = false;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to search VnName");
                IsSearchingForNames = false;
            }

        }

        private async Task<bool> PreSearchCheckAsync()
        {
            if (string.IsNullOrEmpty(VnName) || string.IsNullOrWhiteSpace(VnName)) return false;
            if (VnName.Length < 2) return false;
            CanChangeVnName = false;
            IsResetNameButtonEnabled = false;
            IsResetNameButtonEnabled = false;
            //await ValidateAsync();
            const int retryCount = 5;
            bool didSucceed = false;
            for (int i = 0; i < retryCount; i++)
            {
                if (VndbConnectionTest.VndbTcpSocketTest() == false)
                {
                    App.Logger.Warning("Could not connect to the Vndb API over SSL");
                    await Task.Delay(3500);
                    didSucceed = false;
                }
                else
                {
                    didSucceed = true;
                    break;
                }
            }
            return didSucceed;
        }

        public void ResetName()
        {
            IsNameDropDownOpen = false;
            SuggestedNamesCollection.Clear();
            CanChangeVnName = true;
            IsSearchNameButtonEnabled = true;
        }

        public void BrowseExe()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "Browse for Game",
                DefaultExt = ".exe",
                Filter = "Applications (*.exe)|*.exe",
                FileName = "",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                ExePath = settings.FileName;
            }
        }

        public void ManageExes()
        {
            var multiVm = _container.Get<AddGameMultiViewModel>();
            var result = _windowManager.ShowDialog(multiVm);
            if (result != null && (result == true && multiVm.GameCollection != null))
            {
                ExeCollection.Clear();
                ExeCollection.AddRange(from item in multiVm.GameCollection
                    select new MultiExeGamePaths { ExePath = item.ExePath, IconPath = item.IconPath, ArgumentsString = item.ArgumentsString });
            }
            multiVm.Remove();
        }

        public void BrowseIcon()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "Browse for Game Icon",
                DefaultExt = ".ico",
                Filter = "Icons (*.ico,*.exe)|*.ico;*.exe",
                FileName = "",
                DereferenceLinks = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ValidateNames = true
            };
            bool? result = _dialogService.ShowOpenFileDialog(this, settings);
            if (result == true)
            {
                IconPath = settings.FileName;
            }
        }

        public async Task SubmitAsync()
        {
            IsLockDown = true;
            var parent = (AddGameMainViewModel)Parent;
            parent.CanChangeSource = false;
            bool result = await ValidateAsync();
            if (result == true)
            {
                IsLockDown = false;
                parent.CanChangeSource = true;
                parent.RequestClose(true);
            }

            IsLockDown = false;
            parent.CanChangeSource = true;
        }

        

        public void Cancel()
        {
            var parent = (AddGameMainViewModel)Parent;
            parent.RequestClose();
        }



        private bool IsJapaneseText(string text)
        {
            Regex regex = new Regex(@"/[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B/g");
            return regex.IsMatch(text);
        }


    }


    public class AddGameVndbViewModelValidator : AbstractValidator<AddGameVndbViewModel>
    {
        public AddGameVndbViewModelValidator()
        {

            //VnId
            RuleFor(x => x.VnId).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().When(x => x.IsNameChecked == false)
                    .WithMessage(App.ResMan.GetString("ValidationVnIdNotAboveZero")).MustAsync(IsNotAboveMaxIdAsync)
                    .When(x => x.IsNameChecked == false).WithMessage(App.ResMan.GetString("ValidationVnIdAboveMax"))
                .MustAsync(IsNotDeletedVnAsync).When(x => x.IsNameChecked == false).WithMessage(App.ResMan.GetString("ValidationVnIdDoesNotExist"))
                .Must(IsNotDuplicateId).WithMessage(App.ResMan.GetString("VnIdAlreadyExistsInDb"));

            //Vn name validation
            When(x => x.IsNameChecked == true, () =>
            {
                RuleFor(x => x.CanChangeVnName).NotEqual(true).WithMessage(App.ResMan.GetString("ValidationVnNameSelection"));
                RuleFor(x => x.VnName).Cascade(CascadeMode.StopOnFirstFailure).NotEmpty().When(x => x.CanChangeVnName == false)
                    .WithMessage(App.ResMan.GetString("ValidationVnNameEmpty"))
                    .Must(TrySetNameId).Unless(x => x.IsLockDown == false).WithMessage(App.ResMan.GetString("SetIdFromNameFail"));
            });




            //Exe Path Validation
            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure).ExeValidation()
                .Must(IsNotDuplicateVndbExe).WithMessage(App.ResMan.GetString("ExeAlreadyExistsInDb"));
            
            //Icon
            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure).IcoValidation();
            });


            //Arguments
            When(x => x.IsArgsChecked == true, () =>
            {
                RuleFor(x => x.ExeArguments).Cascade(CascadeMode.StopOnFirstFailure).ArgsValidation();
            });

        }


        private async Task<bool> IsNotDeletedVnAsync(int inputid, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    uint vnid = Convert.ToUInt32(inputid); //17725 is a deleted vn
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid));
                    if (response != null)
                    {
                        return response.Count > 0;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "check for deleted vn failed");
                return false;
            }
        }

        private async Task<bool> IsNotAboveMaxIdAsync(int id, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    RequestOptions ro = new RequestOptions { Reverse = true, Sort = "id", Count = 1 };
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                    if (response != null)
                    {
                        if (id < response.Items[0].Id) return true;
                        else return false;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Could not check max vndb id", ex.Data);
                return false;
            }
        }

        private bool IsNotDuplicateVndbExe(AddGameVndbViewModel instance, string exePath)
        {
            //if type is normal and game id in db or exe in db
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return false;
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    if (instance == null) return false;
                    var id = instance.VnId;
                    var exeType = instance.ExeType;
                    var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query().ToEnumerable();
                    switch (exeType)
                    {
                        case ExeTypeEnum.Normal:
                            {
                                var count = dbUserData.Count(x => x.ExePath == exePath);
                                return count <= 0;
                            }
                        case ExeTypeEnum.Launcher:
                            {
                                var count = dbUserData.Count(x => x.ExePath == exePath);
                                return count <= 0;
                            }
                        case ExeTypeEnum.Collection:
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown Enum Source");
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "IsValidExe");
                throw;
            }
        }

        private bool IsNotDuplicateId(AddGameVndbViewModel instance, int id)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return false;
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    if (instance == null) return false;
                    var exePath = instance.ExePath;
                    var exeType = instance.ExeType;
                    var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query()
                        .Where(x => x.SourceType == AddGameSourceType.Vndb).ToEnumerable();
                    switch (exeType)
                    {
                        case ExeTypeEnum.Normal:
                            {
                                var count = dbUserData.Count(x => x.GameId == id);
                                return count <= 0;
                            }
                        case ExeTypeEnum.Collection:
                            {
                                var count = dbUserData.Count(x => x.GameId == id);
                                return count <= 0;
                            }
                        case ExeTypeEnum.Launcher:
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown Enum Source");
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, $"IsNotDuplicateId encountered an error");
                throw;
            }
        }

        private bool TrySetNameId(AddGameVndbViewModel instance, string name)
        {
            try
            {
                if (instance.VnIdOrName == App.ResMan.GetString("Name"))
                {
                    if (instance.VnNameList == null) return false;
                    if (instance.VnNameList.Count < 0) return false;
                    instance.VnId = (int)instance.VnNameList.Items[instance.DropDownIndex].Id;
                    if (instance.VnId == 0) return false;
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex,"Failed to set ID from selected Vndb Name");
                throw;
            }
        }

    }


}
