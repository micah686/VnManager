using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;

using VisualNovelManagerv2.Model.VisualNovel;

namespace VisualNovelManagerv2.Pages.VisualNovels
{
    /// <summary>
    /// Interaction logic for AddVisualNovel.xaml
    /// </summary>
    public partial class AddVisualNovel : UserControl
    {
       public static Messenger IconMessenger = new Messenger();
        public AddVisualNovel()
        {
            InitializeComponent();
            Messenger.Default.Register<AddVnViewModelService>(this, OpenExeFilePickerDialog);
            IconMessenger.Register<AddVnViewModelService>(this, OpenIconFilePickerDialog);
        }

        private void OpenExeFilePickerDialog(AddVnViewModelService service)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                FileName = "",
                DefaultExt = ".exe",
                Filter = "Applications (*.exe)|*.exe",
                DereferenceLinks = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Browse for Visual Novel Application"
            };
            bool? result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value) return;
            service.PickedFileName = dialog.FileName;
            service.FilePicked?.Invoke();
        }

        private void OpenIconFilePickerDialog(AddVnViewModelService service)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                FileName = "",
                DefaultExt = ".ico",
                Filter = "Icons (*.ico)|*.ico",
                DereferenceLinks = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Browse for Application Icon"
            };
            bool? result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value) return;
            service.PickedFileName = dialog.FileName;
            service.FilePicked?.Invoke();
        }

    }
}
