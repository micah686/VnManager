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
using GalaSoft.MvvmLight.Ioc;
using VisualNovelManagerv2.ViewModel.VisualNovels;

namespace VisualNovelManagerv2.Pages.VisualNovels
{
    /// <summary>
    /// Interaction logic for VnMainCategoryOptions.xaml
    /// </summary>
    public partial class VnMainCategoryOptions : ModernWindow, IClosable
    {
        public VnMainCategoryOptions()
        {
            InitializeComponent();
            this.Unloaded += (o, e) =>
            {
                SimpleIoc.Default.Unregister<VnMainCategoryOptions>();
            };
        }
    }
}
