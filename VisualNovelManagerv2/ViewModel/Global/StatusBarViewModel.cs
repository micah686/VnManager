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

        public BitmapImage DatabaseImage => new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\database.png"));

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


        private static BitmapImage _progressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\ok.png"));
        public BitmapImage ProgressStatus
        {
            get { return _progressStatus; }
            set
            {
                _progressStatus = value;
                RaisePropertyChanged(nameof(ProgressStatus));
            }
        }
    }
}
