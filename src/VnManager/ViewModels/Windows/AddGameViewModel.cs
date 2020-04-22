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
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public bool IsNameChecked { get; set; }



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
