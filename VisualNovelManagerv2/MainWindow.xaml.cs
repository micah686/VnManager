using FirstFloor.ModernUI.Windows.Controls;
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

namespace VisualNovelManagerv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = new Uri("pack://application:,,,/FirstFloor.ModernUI;component/Assets/ModernUI.Dark.xaml");
            InitializeComponent();

#if (!DEBUG)
            DebugLinkGroup.Links.Clear();
            DebugLinkGroup.DisplayName = null;
#endif
        }
    }
}
