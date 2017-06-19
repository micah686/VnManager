using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FirstFloor.ModernUI.Presentation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.Global
{
    public class StatusBarViewModel: ViewModelBase
    {
        public StatusBarViewModel() { }

        #region DatabaseImage
        public BitmapImage DatabaseImage => new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\database.png"));
        #endregion

        #region IsDatabaseProcessing
        private bool _isDbProcessing = false;
        public bool IsDbProcessing
        {
            get { return _isDbProcessing; }
            set
            {
                _isDbProcessing = value;
                RaisePropertyChanged(nameof(IsDbProcessing));
            }
        }
        #endregion

        #region ProgressStatus

        private static BitmapImage _progressStatus;
        public BitmapImage ProgressStatus
        {
            get { return _progressStatus; }
            set
            {
                _progressStatus = value;
                RaisePropertyChanged(nameof(ProgressStatus));
            }
        }
        #endregion

        #region IsWorkProcessing
        private bool _isWorkProcessing;
        public bool IsWorkProcessing
        {
            get { return _isWorkProcessing; }
            set
            {
                _isWorkProcessing = value;
                RaisePropertyChanged(nameof(IsWorkProcessing));
            }
        }
        #endregion

        #region ProgressPercentage
        private double? _progressPercentage;
        public double? ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                if (value != null)
                {
                    value = Math.Round((double)value, 0);
                    _progressPercentage = value;
                    RaisePropertyChanged(nameof(ProgressPercentage));
                }
                else
                {
                    _progressPercentage = null;
                    RaisePropertyChanged(nameof(ProgressPercentage));
                }                                
            }
        }
        #endregion

        #region Message
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }
        #endregion
    }
}
