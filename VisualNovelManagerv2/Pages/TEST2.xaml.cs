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

namespace VisualNovelManagerv2.Pages
{
    /// <summary>
    /// Interaction logic for TEST2.xaml
    /// </summary>
    public partial class TEST2 : UserControl
    {
        public TEST2()
        {
            InitializeComponent();
            Globals.StatusBar.IsWorkProcessing = true;
        }

        public new bool IsVisible = true;
    }
}
