using System;
using System.Globalization;
using System.Windows.Data;
using Stylet;

namespace VnManager.ViewModels.Windows
{
    public class AddGameViewModel: Screen
    {
        public string SourceSite { get; set; }
        public int VnId { get; set; }        
        public string VnName { get; set; }
        public bool IsNameChecked { get; set; }        
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        private bool _isExeNormalChecked;
        public bool IsExeNormalChecked
        {
            get { return _isExeNormalChecked; }
            set
            {
                if(value == true)
                {
                    IsNotExeCollection = true;
                }
                
                SetAndNotify(ref _isExeNormalChecked, value);
            }
        }
        private bool _isExeLauncherChecked;
        public bool IsExeLauncherChecked
        {
            get { return _isExeLauncherChecked; }
            set
            {
                if (value == true)
                {
                    IsNotExeCollection = true;
                }
                SetAndNotify(ref _isExeLauncherChecked, value);
            }
        }
        private bool _isExeCollectionChecked;
        public bool IsExeCollectionChecked
        {
            get { return _isExeCollectionChecked; }
            set
            {
                if (value == true)
                {
                    IsNotExeCollection = false;
                }
                SetAndNotify(ref _isExeCollectionChecked, value);
            }
        }
        public bool IsCustomArgsChecked { get; set; }
        public bool IsCustomIconChecked { get; set; }
        public string ExeArguments { get; set; }

        private bool _isNotExeCollection;
        public bool IsNotExeCollection
        {
            get { return _isNotExeCollection; }
            set
            {
                ExeArguments = string.Empty;
                IconPath = string.Empty;
                IsCustomArgsChecked = false;
                IsCustomIconChecked = false;
                SetAndNotify(ref _isNotExeCollection, value);
            }
        }



        private readonly IWindowManager _windowManager;

        public AddGameViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
    }

    public class VnIdNameBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
                return "Vn Name:";
            else
                return "Vn ID:";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
