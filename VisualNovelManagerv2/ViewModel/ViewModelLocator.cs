/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:ServicesSample.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using VisualNovelManagerv2.ViewModel.Global;
using VisualNovelManagerv2.ViewModel.Settings;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VisualNovelManagerv2.ViewModel.VisualNovels.AddVn;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnCharacter;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnListViewModel;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMain;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMainCategoryOptions;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnRelease;

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
            SimpleIoc.Default.Register<VnScreenshotViewModel>();
            SimpleIoc.Default.Register<VnCharacterViewModel>();
            SimpleIoc.Default.Register<VnReleaseViewModel>();
            SimpleIoc.Default.Register<StatusBarViewModel>();
            SimpleIoc.Default.Register<VnListViewModel>();
            SimpleIoc.Default.Register<UserSettingsViewModel>();
            SimpleIoc.Default.Register<VnMainCategoryOptionsViewModel>();
        }

        public AddVnViewModel AddVn => ServiceLocator.Current.GetInstance<AddVnViewModel>();

        public VnMainViewModel VnMain => ServiceLocator.Current.GetInstance<VnMainViewModel>();

        public VnScreenshotViewModel VnScreenshot => ServiceLocator.Current.GetInstance<VnScreenshotViewModel>();

        public VnCharacterViewModel VnCharacter => ServiceLocator.Current.GetInstance<VnCharacterViewModel>();

        public VnReleaseViewModel VnRelease => ServiceLocator.Current.GetInstance<VnReleaseViewModel>();

        public StatusBarViewModel StatusBar => ServiceLocator.Current.GetInstance<StatusBarViewModel>();

        public VnListViewModel VnList => ServiceLocator.Current.GetInstance<VnListViewModel>();

        public UserSettingsViewModel UserSettings => ServiceLocator.Current.GetInstance<UserSettingsViewModel>();

        public VnMainCategoryOptionsViewModel VnMainCategoryOptionsViewModel =>
            ServiceLocator.Current.GetInstance<VnMainCategoryOptionsViewModel>();

        public static void CleanupScreenshotViewModel()
        {
            SimpleIoc.Default.Unregister<VnScreenshotViewModel>();
            SimpleIoc.Default.Register<VnScreenshotViewModel>();
        }
    }
}
