/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:ServicesSample.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using VisualNovelManagerv2.ViewModel.VisualNovels;

namespace VisualNovelManagerv2.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<AddVnViewModel>();
            SimpleIoc.Default.Register<VnMainViewModel>();
        }

        public AddVnViewModel AddVn
        {
            get { return ServiceLocator.Current.GetInstance<AddVnViewModel>(); }
        }

        public VnMainViewModel VnMain
        {
            get { return ServiceLocator.Current.GetInstance<VnMainViewModel>(); }
        }

        public static void Cleanup()
        { }
    }
}
