using System;
using System.Globalization;
using System.Windows.Data;
using Stylet;
using MvvmDialogs;
using FluentValidation;
using StyletIoC;
using VnManager.ViewModels.Dialogs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using VnManager.Helpers;
using System.Threading;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models.VisualNovel;
using VndbSharp.Models;
using VnManager.Helpers.Vndb;
using VnManager.Utilities;
using System.Text.RegularExpressions;
using LiteDB;
using VndbSharp.Models.Errors;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        internal readonly List<MultiExeGamePaths> ExeCollection = new List<MultiExeGamePaths>();
        public BindableCollection<string> SuggestedNamesCollection { get; private set; }

        #region Properties
        public string SourceSite { get; set; }
        public int VnId { get; set; }        
        public string VnName { get; set; }
        public bool IsNameChecked { get; set; }        
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        public bool CanChangeVnName { get; set; }
        public bool HideArgumentsError { get; private set; } = false;
        public bool HideIconError { get; private set; } = false;
        public bool IsNameDropDownOpen { get; set; } = false;
        public string SelectedName { get; set; }
        public bool IsSearchingForNames { get; set; } = false;
        public bool IsSearchNameButtonEnabled { get; set; } = true;
        public bool IsResetNameButtonEnabled { get; set; } = true;


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
                    HideIconError = true;
                    ValidateAsync();
                    HideIconError = false;
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
                    HideArgumentsError = true;
                    ValidateAsync();
                    HideArgumentsError = false;
                }
            }
        }

        private ExeTypesEnum _exeTypes;
        public ExeTypesEnum ExeTypes
        {
            get { return _exeTypes; }
            set
            {
                SetAndNotify(ref _exeTypes, value);
                if(_exeTypes != ExeTypesEnum.Collection)
                {
                    IsNotExeCollection = true;
                }
                else
                {
                    IsNotExeCollection = false;
                }
            }
        }

        public AddGameSourceTypes SourceTypes { get; set; } = AddGameSourceTypes.Vndb;

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

        #endregion



        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IContainer _container;
        public AddGameViewModel(IContainer container, IWindowManager windowManager, IModelValidator<AddGameViewModel> validator, IDialogService dialogService) : base(validator)
        {
            _container = container;
            _windowManager = windowManager;
            _dialogService = dialogService;
            IsNotExeCollection = true;
            CanChangeVnName = true;
            SuggestedNamesCollection = new BindableCollection<string>();
        }

        public async Task Search()
        {
            if (string.IsNullOrEmpty(VnName) || string.IsNullOrWhiteSpace(VnName)) return;
            CanChangeVnName = false;
            IsSearchNameButtonEnabled = false;
            IsResetNameButtonEnabled = false;
            await ValidateAsync();
            if (VndbConnectionTest.VndbTcpSocketTest() == false)
            {
                App.Logger.Warning("Could not connect to the Vndb API over SSL");
                await Task.Delay(3500);
            }
            try
            {
                if (VnName == null || VnName.Length < 2) return;
                using (Vndb client = new Vndb(true))
                {
                    var stopwatch = new Stopwatch();
                    bool shouldContinue = true;
                    var maxTime = TimeSpan.FromMinutes(1.5);
                    SuggestedNamesCollection.Clear();
                    VndbResponse<VisualNovel> vnNameList = null;
                    IsSearchingForNames = true;

                    stopwatch.Start();
                    while (shouldContinue)
                    {
                        shouldContinue = false;
                        vnNameList = await client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(null), VndbFlags.Basic);
                        //do I need to check for null
                        if (vnNameList.Count < 1 && client.GetLastError().Type == ErrorType.Throttled)
                        {
                            if (stopwatch.Elapsed > maxTime) return;
                            await HandleVndbErrors.ThrottledWait((ThrottledError)client.GetLastError(), 0);
                            shouldContinue = true;
                        }
                        else if (vnNameList.Count < 1)
                        {
                            HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                            return;
                        }
                        else
                        {
                            if (stopwatch.Elapsed > maxTime) return;
                            List<string> nameList = IsJapaneseText(VnName) == true ? vnNameList.Select(item => item.OriginalName).ToList() : vnNameList.Select(item => item.Name).ToList();
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
                throw;
            }
        }

        private bool IsJapaneseText(string text)
        {
            Regex regex = new Regex(@"/[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B/g");
            return regex.IsMatch(text);
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
            var multivm = _container.Get<AddGameMultiViewModel>();
            var result = _windowManager.ShowDialog(multivm).Value;
            if(result == true && multivm.GameCollection != null)
            {
                ExeCollection.Clear();
                ExeCollection.AddRange(from item in multivm.GameCollection
                                        select new MultiExeGamePaths { ExePath = item.ExePath, IconPath = item.IconPath, ArgumentsString = item.ArgumentsString });
            }
            multivm.Remove();
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

        public void Submit()
        {
            var validator = new AddGameViewModelValidator();
            ValidateAsync();
            bool result = validator.Validate(this).IsValid;
            if(result == true)
            {
                this.RequestClose();
            }
        }

        public void Cancel()
        {
            RequestClose();
        }


    }

    public enum ExeTypesEnum { Normal, Launcher, Collection}
    public enum AddGameSourceTypes { NoSource, Vndb}

    public class AddGameViewModelValidator : AbstractValidator<AddGameViewModel>
    {
        public AddGameViewModelValidator()
        {
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            RuleFor(x => x.VnId).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).When(x => x.IsNameChecked == false)
                    .WithMessage(rm.GetString("ValidationVnIdNotAboveZero"))
                .MustAsync(IsNotAboveMaxId).Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb)
                    .When(x => x.IsNameChecked == false).WithMessage(rm.GetString("ValidationVnIdAboveMax"))
                .MustAsync(IsNotDeletedVn).Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb)
                    .When(x => x.IsNameChecked == false).WithMessage(rm.GetString("ValidationVnIdDoesNotExist"))
                .Must(IsNotDuplicateId).WithMessage(rm.GetString("VnIdAlreadyExistsInDb"));

            When(x => x.IsNameChecked == true, () =>
            {
                RuleFor(x => x.CanChangeVnName).NotEqual(true).Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).WithMessage(rm.GetString("ValidationVnNameSelection"));
                RuleFor(x => x.VnName).NotEmpty().Unless(x => x.SourceTypes != AddGameSourceTypes.Vndb).When(x => x.CanChangeVnName ==false).WithMessage(rm.GetString("ValidationVnNameEmpty"));
            });


            RuleFor(x => x.ExePath).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage(rm.GetString("ValidationExePathEmpty"))
                .Must(ValidateFiles.EndsWithExe).WithMessage(rm.GetString("ValidationExePathNotValid"))
                .Must(ValidateFiles.ValidateExe).WithMessage(rm.GetString("ValidationExeNotValid"))
                .Must(IsNotDuplicateExe).WithMessage(rm.GetString("ExeAlreadyExistsInDb"));


            RuleFor(x => x.IconPath).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().When(x => x.IsIconChecked == true).Unless(x => x.HideIconError == true).WithMessage(rm.GetString("ValidationIconPathEmpty"))
                .Must(ValidateFiles.EndsWithIcoOrExe).When(x => x.IsIconChecked == true).Unless(x => x.HideIconError == true).WithMessage(rm.GetString("ValidationIconPathNotValid"));

            RuleFor(x => x.ExeArguments).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().When(x => x.IsArgsChecked == true && x.ExeArguments == "").Unless(x => x.HideArgumentsError == true).WithMessage(rm.GetString("ValidationArgumentsEmpty"))
                .Must(AddGameMultiViewModelValidator.ContainsIllegalCharacters).When(x => x.IsArgsChecked == true && x.ExeArguments == "")
                        .Unless(x => x.HideArgumentsError == true).WithMessage(rm.GetString("ValidationArgumentsIllegalChars"));

        }


        #region Validators

        private async Task<bool> IsNotDeletedVn(int inputid, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    uint vnid = Convert.ToUInt32(inputid); //17725 is deleted vn
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid));
                    if (response != null)
                    {
                        return response.Count > 0;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
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

        private async Task<bool> IsNotAboveMaxId(int id, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    RequestOptions ro = new RequestOptions { Reverse = true, Sort = "id",Count = 1 };
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                    if (response != null)
                    {
                        if (id < response.Items[0].Id) return true;
                        else return false;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
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

        private bool IsNotDuplicateExe(AddGameViewModel instance, string exePath)
        {
            //if type is normal and game id in db or exe in db
            try
            {
                using (var db = new LiteDatabase(App.DatabasePath))
                {
                    if (instance == null) return false;
                    var id = instance.VnId;
                    var exeType = instance.ExeTypes;
                    var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query()
                        .Where(x => x.SourceType == AddGameSourceTypes.NoSource).ToEnumerable();
                    switch (exeType)
                    {
                        case ExeTypesEnum.Normal:
                        {
                            var count = dbUserData.Count(x => x.ExePath == exePath || x.GameId == id);
                            return count <= 0;
                        }
                        case ExeTypesEnum.Launcher:
                        {
                            var count = dbUserData.Count(x => x.ExePath == exePath);
                            return count <= 0;
                        }
                        case ExeTypesEnum.Collection:
                            return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "IsValidExe");
                throw;
            }
        }


        private bool IsNotDuplicateId(AddGameViewModel instance, int id)
        {
            try
            {
                using (var db = new LiteDatabase(App.DatabasePath))
                {
                    if (instance == null) return false;
                    var exePath = instance.ExePath;
                    var exeType = instance.ExeTypes;
                    var dbUserData = db.GetCollection<UserDataGames>("UserData_Games").Query()
                        .Where(x => x.SourceType == AddGameSourceTypes.NoSource).ToEnumerable();
                    switch (exeType)
                    {
                        case ExeTypesEnum.Normal:
                        {
                            var count = dbUserData.Count(x => x.GameId == id || x.ExePath == exePath);
                            return count <= 0;
                        }
                        case ExeTypesEnum.Collection:
                        {
                            var count = dbUserData.Count(x => x.GameId == id);
                            return count <= 0;
                        }
                        case ExeTypesEnum.Launcher:
                            return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #endregion
    }

    public class VnIdNameBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            if (value != null && (bool)value)
                return rm.GetString("VnName");
            else
                return rm.GetString("VnId");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
}
