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

namespace VisualNovelManagerv2.Pages.VisualNovels
{
    /// <summary>
    /// Interaction logic for VnMain.xaml
    /// </summary>
    public partial class VnMain : UserControl
    {
        public VnMain()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }
        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "Show Add/Remove Category Window")
            {
                var view2 = new VnMainCategoryOptions();
                view2.ShowDialog();
            }
        }
    }
}
