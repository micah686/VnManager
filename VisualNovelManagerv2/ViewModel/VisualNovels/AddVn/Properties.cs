using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using Microsoft.Expression.Interactivity.Core;
using VisualNovelManagerv2.Design.VisualNovel;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.AddVn
{
    public partial class AddVnViewModel
    {
        #region Static Properties

        #region VnId
        private uint? _inputVnId;
        public uint? InputVnId
        {
            get { return _inputVnId; }
            set
            {
                _inputVnId = value;
                RaisePropertyChanged(nameof(InputVnId));
                if (InputVnId == null) return;
                Validator.ValidateAsync(InputVnId);
            }
        }
        #endregion VnId

        #region FileName
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged(nameof(FileName));
                Validator.ValidateAsync(FileName);

            }
        }
        #endregion FileName

        #region IconName
        private string _iconName;
        public string IconName
        {
            get { return _iconName; }
            set
            {
                _iconName = value;
                RaisePropertyChanged(nameof(IconName));
                Validator.ValidateAsync(IconName);
            }
        }
        #endregion IconName

        #region VnName
        private string _vnName;
        public string VnName
        {
            get { return _vnName; }
            set
            {
                _vnName = value;
                RaisePropertyChanged(nameof(VnName));
                if (VnName == null) return;
                Validator.ValidateAsync(VnName);
            }
        }
        #endregion VnName        

        #region IsNameChecked
        private bool _isNameChecked;
        public bool IsNameChecked
        {
            get { return _isNameChecked; }
            set
            {
                _isNameChecked = value;
                VnName = null;
                InputVnId = null;
                RaisePropertyChanged(nameof(IsNameChecked));
            }
        }
        #endregion IsNameChecked

        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
        #endregion IsChecked

        #region IsRunning
        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                RaisePropertyChanged(nameof(IsRunning));
            }
        }
        #endregion IsRunning

        #region IsDropDownOpen
        private bool _isDropDownOpen;
        public bool IsDropDownOpen
        {
            get { return _isDropDownOpen; }
            set
            {
                _isDropDownOpen = value;
                RaisePropertyChanged(nameof(IsDropDownOpen));
            }
        }
        #endregion IsDropDownOpen

        #region DropdownIndex
        private int _dropdownIndex;
        public int DropdownIndex
        {
            get { return _dropdownIndex; }
            set
            {
                _dropdownIndex = value;
                RaisePropertyChanged(nameof(DropdownIndex));
            }
        }
        #endregion DropDownIndex

        #region IsIconEnabled
        private bool _isIconEnabled;
        public bool IsIconEnabled
        {
            get { return _isIconEnabled; }
            set
            {
                _isIconEnabled = value;
                RaisePropertyChanged(nameof(IsIconEnabled));
            }
        }
        #endregion IsIconEnabled

        #region IsValid
        private bool? _isValid;
        public bool? IsValid
        {
            get { return _isValid; }
            private set
            {
                _isValid = value;
                RaisePropertyChanged(nameof(IsValid));
            }
        }
        #endregion IsValid

        #region ValidationErrorsString
        private string _validationErrorsString;
        public string ValidationErrorsString
        {
            get { return _validationErrorsString; }
            private set
            {
                _validationErrorsString = value;
                RaisePropertyChanged(nameof(ValidationErrorsString));
            }
        }
        #endregion ValidationErrorsString

        private uint _vnid = 0;
        private double _progressIncrement = 0;
        private KeyValuePair<bool, DateTime> _didDownloadTagDump = new KeyValuePair<bool, DateTime>(true, DateTime.Now);
        private VndbResponse<VisualNovel> _vnNameList;
        private readonly AddVnViewModelService _exeService;
        private readonly AddVnViewModelService _iconService;
        public RelayCommand GetFile { get; }
        public RelayCommand GetIcon { get; }
        public ObservableCollection<string> SuggestedNamesCollection { get; set; }
        public ICommand ValidateCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(Validate);
        public ICommand SearchNamesCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(SearchName);
        public ICommand GetVnDataCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(GetData);
        #endregion

    }
}
