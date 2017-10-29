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
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;

namespace VisualNovelManagerv2.Pages.Links.Settings
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettings : UserControl
    {
        public UserSettings()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }
        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "Saved Settings")
            {
                ModernDialog.ShowMessage(
                    "Your settings were saved", "Saved Settings",
                    MessageBoxButton.OK);
            }
        }
    }
}
