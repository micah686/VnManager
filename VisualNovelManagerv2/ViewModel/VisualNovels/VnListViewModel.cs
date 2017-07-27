using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FirstFloor.ModernUI.Presentation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnListViewModel: ViewModelBase
    {
        #region Properties

        #region VotelistCollection
        private ObservableCollection<string> _votelistCollection;
        public ObservableCollection<string> VotelistCollection
        {
            get { return _votelistCollection; }
            set
            {
                _votelistCollection = value;
                RaisePropertyChanged(nameof(VotelistCollection));
            }
        }        
        #endregion

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
        private SecureString _password;
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



        #endregion

        public ICommand LoginCommand => new GalaSoft.MvvmLight.Command.RelayCommand(Login);
        public VnListViewModel() { }

        private void Login()
        {
            
        }
    }
}
