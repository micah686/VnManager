using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.Design.VisualNovel;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnListViewModel
{
    public partial class VnListViewModel
    {
        #region VnLinksModel
        private VnLinksModel _vnLinksModel;
        public VnLinksModel VnLinksModel
        {
            get { return _vnLinksModel; }
            set
            {
                _vnLinksModel = value;
                RaisePropertyChanged(nameof(VnLinksModel));
            }
        }
        #endregion

        //groups
        #region Collections
        #region UserListCollection
        private RangeEnabledObservableCollection<string> _userListCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> UserListCollection
        {
            get { return _userListCollection; }
            set
            {
                _userListCollection = value;
                RaisePropertyChanged(nameof(UserListCollection));
            }
        }
        #endregion

        #region VoteCollection
        private ObservableCollection<string> _voteCollection = new ObservableCollection<string>();
        public ObservableCollection<string> VoteCollection
        {
            get { return _voteCollection; }
            set
            {
                _voteCollection = value;
                RaisePropertyChanged(nameof(VoteCollection));
            }
        }
        #endregion

        #region VnListCollection
        private ObservableCollection<string> _vnListCollection = new ObservableCollection<string>();
        public ObservableCollection<string> VnListCollection
        {
            get { return _vnListCollection; }
            set
            {
                _vnListCollection = value;
                RaisePropertyChanged(nameof(VnListCollection));
            }
        }
        #endregion

        #region WishlistCollection
        private ObservableCollection<string> _wishlistCollection = new ObservableCollection<string>();
        public ObservableCollection<string> WishlistCollection
        {
            get { return _wishlistCollection; }
            set
            {
                _wishlistCollection = value;
                RaisePropertyChanged(nameof(WishlistCollection));
            }
        }
        #endregion
        #endregion Collections

        #region Information
        #region InfoStatus
        private string _infoStatus;
        public string InfoStatus
        {
            get { return _infoStatus; }
            set
            {
                _infoStatus = value;
                RaisePropertyChanged(nameof(InfoStatus));
            }
        }
        #endregion

        #region InfoAdded
        private string _infoAdded;
        public string InfoAdded
        {
            get { return _infoAdded; }
            set
            {
                _infoAdded = value;
                RaisePropertyChanged(nameof(InfoAdded));
            }
        }
        #endregion

        #region InfoVote
        private double _infoVote;
        public double InfoVote
        {
            get { return _infoVote; }
            set
            {
                _infoVote = value;
                RaisePropertyChanged(nameof(InfoVote));
            }
        }
        #endregion

        #region InfoNote
        private string _infoNote;
        public string InfoNote
        {
            get { return _infoNote; }
            set
            {
                _infoNote = value;
                RaisePropertyChanged(nameof(InfoNote));
            }
        }
        #endregion

        #region InfoPriority
        private string _infoPriority;
        public string InfoPriority
        {
            get { return _infoPriority; }
            set
            {
                _infoPriority = value;
                RaisePropertyChanged(nameof(InfoPriority));
            }
        }
        #endregion
        #endregion Information

        #region Set Command Properties
        #region VoteDropDownSelected
        private string _voteDropDownSelected;
        public string VoteDropDownSelected
        {
            get { return _voteDropDownSelected; }
            set
            {
                _voteDropDownSelected = value;
                RaisePropertyChanged(nameof(VoteDropDownSelected));
            }
        }
        #endregion

        #region VotelistVote
        private string _votelistVote;
        public string VotelistVote
        {
            get { return _votelistVote; }
            set
            {
                _votelistVote = value;
                RaisePropertyChanged(nameof(VotelistVote));
            }
        }
        #endregion

        #region VnList Status
        private string _vnlistStatus;
        public string VnListStatus
        {
            get { return _vnlistStatus; }
            set
            {
                _vnlistStatus = value;
                RaisePropertyChanged(nameof(VnListStatus));
            }
        }
        #endregion

        #region NoteEnabled
        private bool _noteEnabled;
        public bool NoteEnabled
        {
            get { return _noteEnabled; }
            set
            {
                _noteEnabled = value;
                RaisePropertyChanged(nameof(NoteEnabled));
            }
        }
        #endregion

        #region VnList Note
        private string _vnListNote;
        public string VnListNote
        {
            get { return _vnListNote; }
            set
            {
                _vnListNote = value;
                RaisePropertyChanged(nameof(VnListNote));
            }
        }
        #endregion

        #region WishlistPriority
        private string _wishlistPriority;
        public string WishlistPriority
        {
            get { return _wishlistPriority; }
            set
            {
                _wishlistPriority = value;
                RaisePropertyChanged(nameof(WishlistPriority));
            }
        }
        #endregion
        #endregion Set Command Properties

        #region Validation Properties
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
        #endregion

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
        #endregion
        #endregion Validation Properties
        //end groups

        #region Username
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged(nameof(Username));
            }
        }
        #endregion

        #region Password
        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged(nameof(Password));
            }
        }
        #endregion

        #region SelectedItem
        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                if (value != null)
                {
                    BindImage();
                    if (IsVoteListSelected)
                        BindVoteData();
                    else if (IsVnListSelected)
                        BindVnList();
                    else if (IsWishlistSelected)
                        BindWishList();
                }
            }
        }
        #endregion

        #region IsVoteListSelected

        private bool _isVoteListSelected = false;
        public bool IsVoteListSelected
        {
            get { return _isVoteListSelected; }
            set
            {
                _isVoteListSelected = value;
                RaisePropertyChanged(nameof(IsVoteListSelected));
            }
        }
        #endregion

        #region IsWishlistSelected
        private bool _isWishlistSelected;
        public bool IsWishlistSelected
        {
            get { return _isWishlistSelected; }
            set
            {
                _isWishlistSelected = value;
                RaisePropertyChanged(nameof(IsWishlistSelected));
            }
        }
        #endregion

        #region IsVnListSelected
        private bool _isVnListSelected;
        public bool IsVnListSelected
        {
            get { return _isVnListSelected; }
            set
            {
                _isVnListSelected = value;
                RaisePropertyChanged(nameof(IsVnListSelected));
            }
        }
        #endregion

        #region IsUserInputEnabled
        private bool _isUserInputEnabled = true;
        public bool IsUserInputEnabled
        {
            get { return _isUserInputEnabled; }
            set
            {
                _isUserInputEnabled = value;
                RaisePropertyChanged(nameof(IsUserInputEnabled));
            }
        }
        #endregion

        #region MaxWidth

        private double _maxWidth = 250;
        public double MaxWidth
        {
            get { return _maxWidth; }
            set
            {
                _maxWidth = value;
                RaisePropertyChanged(nameof(MaxWidth));
            }
        }
        #endregion

        private uint _userId = 0;
        private uint _vnId = 0;

    }
}
