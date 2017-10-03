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
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMain;

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
            Messenger.Default.Register<NotificationMessageAction<MessageBoxResult>>(this, NotificationMessageBoxResultRecieved);
        }
        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "Show Add/Remove Category Window")
            {
                var view2 = new VnMainCategoryOptions();
                view2.ShowDialog();
            }
            if (msg.Notification == "Game Already Running")
            {
               ModernDialog.ShowMessage(
                    "A game is already running. Please close any other running games, then try again", "Game Already Running",
                    MessageBoxButton.OK);
            }
        }

        private void NotificationMessageBoxResultRecieved(NotificationMessageAction<MessageBoxResult> msg)
        {
            if (msg.Notification == "Delete Vn Confirm")
            {
                var result = ModernDialog.ShowMessage("Are you SURE you want to delete this visual novel and all associated data?", "Delete Visual Novel?", MessageBoxButton.YesNo);
                msg.Execute(result);
            }
            
        }
        
    }
}
