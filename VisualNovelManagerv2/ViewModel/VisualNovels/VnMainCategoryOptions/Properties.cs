using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualNovelManagerv2.CustomClasses;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMainCategoryOptions
{
    public partial class VnMainCategoryOptionsViewModel
    {
        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                AddCategoryText = null;
                RemoveCategoryText = null;
                CategoriesCollection.Clear();
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
        #endregion

        #region AddCategoryText
        private string _addCategoryText;
        public string AddCategoryText
        {
            get { return _addCategoryText; }
            set
            {
                _addCategoryText = value;
                if (value != null)
                {
                    _addCategoryText = value.Trim();
                }
                RaisePropertyChanged(nameof(AddCategoryText));
            }
        }
        #endregion

        #region RemoveCategoryText
        private string _removeCategoryText;
        public string RemoveCategoryText
        {
            get { return _removeCategoryText; }
            set
            {
                _removeCategoryText = value;
                RaisePropertyChanged(nameof(RemoveCategoryText));
            }
        }
        #endregion

        #region BbCodeTextDone
        private string _bbCodeTextDone;
        public string BbCodeTextDone
        {
            get { return _bbCodeTextDone; }
            set
            {
                _bbCodeTextDone = value;
                RaisePropertyChanged(nameof(BbCodeTextDone));
            }
        }
        #endregion

        #region IsDropdownOpen
        private bool _isDropdownOpen;
        public bool IsDropdownOpen
        {
            get { return _isDropdownOpen; }
            set
            {
                _isDropdownOpen = value;
                RaisePropertyChanged(nameof(IsDropdownOpen));
            }
        }
        #endregion

        #region CategoriesCollection
        private RangeEnabledObservableCollection<string> _categoriesCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> CategoriesCollection
        {
            get { return _categoriesCollection; }
            set
            {
                _categoriesCollection = value;
                RaisePropertyChanged(nameof(CategoriesCollection));
            }
        }
        #endregion

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
        public ICommand LoadCategoryListCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadCategoryList);
        public ICommand SumbitCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<IClosable>(OnSubmitClicked);
        public ICommand CancelCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<IClosable>(OnCloselClicked);
        private bool _isValid = false;
    }
}
