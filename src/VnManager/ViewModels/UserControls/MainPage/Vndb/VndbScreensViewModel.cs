using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbScreensViewModel:Screen
    {
        private readonly IContainer _container;

        #region SelectedScreenIndex
        private int _selectedScreenIndex = -1;
        public int SelectedScreenIndex
        {
            get => _selectedScreenIndex;
            set
            {
                SetAndNotify(ref _selectedScreenIndex, value);
                if (_selectedScreenIndex < 0)
                {
                    _selectedScreenIndex = 0;
                }
                LoadLargeScreenshot();
            }
        }
        #endregion

        public BitmapSource MainImage { get; set; }
        public BindableCollection<BitmapSource> ScreenshotCollection = new BindableCollection<BitmapSource>();

        private readonly List<ScreenShot> _screenList = LoadScreenshotList();

        public VndbScreensViewModel(IContainer container)
        {
            _container = container;
        }

        protected override async void OnViewLoaded()
        {
            await ResetInvalidScreenshots();
        }

        public void ShowInfo()
        {
            var vm = VndbContentViewModel.Instance;
            vm.ActivateVnInfo();
        }

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
        }


        private static List<ScreenShot> LoadScreenshotList()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return null;
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString()).Query()
                .Where(x => x.VnId == VndbContentViewModel._vnid).ToEnumerable();
            var scrList = dbUserData.Select(item => new ScreenShot {Uri = item.ImageUri, IsNsfw = NsfwHelper.IsNsfw(item.ImageRating)}).ToList();
            return scrList;
        }


        private async Task ResetInvalidScreenshots()
        {
            List<ScreenShot> scrExistList = new List<ScreenShot>();
            List<string> fileExistList = new List<string>();
            string mainDir = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\{VndbContentViewModel._vnid}";
            foreach (var item in _screenList)
            {
                string fileName = Path.GetFileName(item.Uri.AbsoluteUri);
                switch (item.IsNsfw)
                {
                        
                    case true when File.Exists($@"{mainDir}\{fileName}.aes"):
                        scrExistList.Add(item);
                        fileExistList.Add($@"{mainDir}\{fileName}.aes");
                        break;
                    case false when File.Exists($@"{mainDir}\{fileName}"):
                        scrExistList.Add(item);
                        fileExistList.Add($@"{mainDir}\{fileName}");
                        break;
                }
            }

            foreach (var file in Directory.EnumerateFiles(mainDir))
            {
                if (fileExistList.Contains(file)) continue;
                File.Delete(file);

            }

            if (Directory.Exists($@"{mainDir}\thumbs"))
            {
                foreach (var file in Directory.EnumerateFiles($@"{mainDir}\thumbs"))
                {
                    if (fileExistList.Contains(file)) continue;
                    File.Delete(file);
                }
            }

            var downloadList = _screenList.Except(scrExistList).ToList();
            await ImageHelper.DownloadImagesWithThumbnailsAsync(downloadList, mainDir);
        }

        private void LoadLargeScreenshot()
        {
            try
            {
                List<ScreenShot> screenshotList = LoadScreenshotList();
                if (screenshotList.Count <= 0) return;
                string path = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\
                        {VndbContentViewModel._vnid}\{Path.GetFileName(screenshotList[SelectedScreenIndex].Uri.AbsoluteUri)}";
                switch (screenshotList[SelectedScreenIndex].IsNsfw)
                {
                    case true:
                        if (File.Exists($"{path}.aes"))
                        {
                            var imgBytes = File.ReadAllBytes($"{path}.aes");
                            var imgStream = Secure.DecStreamToStream(new MemoryStream(imgBytes));
                            MainImage = ImageHelper.CreateBitmapFromStream(imgStream);
                        }
                        else
                        {
                            MainImage = ImageHelper.CreateBitmapFromPath(path);
                        }
                        break;
                    case false:

                        MainImage = ImageHelper.CreateBitmapFromPath(path);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        private void BindScreenshotCollection()
        {
            List<ScreenShot> screenshotList = LoadScreenshotList();
            foreach (var item in screenshotList)
            {
                var image = ImageHelper.CreateEmptyBitmapImage();
                if (screenshotList.Count < 1) return;
                string path = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\
                        {VndbContentViewModel._vnid}\{Path.GetFileName(item.Uri.AbsoluteUri)}";
                switch (item.IsNsfw)
                {
                    case true when File.Exists($"{path}.aes"):
                        var imgBytes = File.ReadAllBytes($"{path}.aes");
                        var imgStream = Secure.DecStreamToStream(new MemoryStream(imgBytes));
                        image= ImageHelper.CreateBitmapFromStream(imgStream);
                        break;
                    case false when File.Exists(path):
                        image = ImageHelper.CreateBitmapFromPath(path);
                        break;
                }
                ScreenshotCollection.Add(image);
            }
        }

    }
}
