using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Stylet;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Main;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbScreensViewModel : Screen
    {
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

        public BindingImage MainImage { get; set; }

        public BindableCollection<BindingImage> ScreenshotCollection { get; set; } = new BindableCollection<BindingImage>();

        private List<BindingImage> _scrList = new List<BindingImage>();
        private bool _finishedLoad = false;

        protected override void OnViewLoaded()
        {
            if (_finishedLoad == true)
            {
                return;
            }
            if (_scrList.Count > 0)
            {
                return;
            }
            _scrList = LoadScreenshotList();
            BindScreenshotCollection();
            _finishedLoad = true;
            if (ScreenshotCollection.Count > 0)
            {
                SelectedScreenIndex = 0;
            }
        }

        private List<BindingImage> LoadScreenshotList()
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1)
            {
                return new List<BindingImage>();
            }
            using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
            var dbUserData = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString()).Query()
                .Where(x => x.VnId == VndbContentViewModel.VnId).ToEnumerable();
            var scrList = dbUserData.Select(item => new BindingImage { ImageLink = item.ImageLink, Rating = item.ImageRating }).ToList();
            return scrList;
        }


        private void LoadLargeScreenshot()
        {
            const int blurWeight = 20;
            try
            {
                List<BindingImage> screenshotList = _scrList;
                if (screenshotList.Count <= 0)
                {
                    return;
                }
                if (SelectedScreenIndex < 0)
                {
                    return;
                }
                string path = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\{VndbContentViewModel.VnId}\{Path.GetFileName(screenshotList[SelectedScreenIndex].ImageLink)}";
                var rating = NsfwHelper.RawRatingIsNsfw(screenshotList[SelectedScreenIndex].Rating);
                var userIsNsfw = NsfwHelper.UserIsNsfw(screenshotList[SelectedScreenIndex].Rating);
                if (rating == true && File.Exists($"{path}.aes"))
                {
                    var imgBytes = File.ReadAllBytes($"{path}.aes");
                    var imgStream = Secure.DecStreamToStream(new MemoryStream(imgBytes));
                    var imgNsfw = ImageHelper.CreateBitmapFromStream(imgStream);
                    MainImage = CreateBlurBindingImage(imgNsfw, userIsNsfw, blurWeight);
                }
                else
                {
                    var img = ImageHelper.CreateBitmapFromPath(path);
                    MainImage = CreateBlurBindingImage(img, userIsNsfw, blurWeight);
                }
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to load large screenshot");
                throw;
            }

        }

        private void BindScreenshotCollection()
        {
            const int blurWeight = 10;
            List<BindingImage> screenshotList = _scrList;
            List<BindingImage> toDelete = new List<BindingImage>();
            foreach (var item in screenshotList)
            {
                BitmapSource image;
                if (screenshotList.Count < 1)
                {
                    return;
                }
                string thumbPath = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\{VndbContentViewModel.VnId}\thumbs\{Path.GetFileName(item.ImageLink)}";
                string imagePath = $@"{App.AssetDirPath}\sources\vndb\images\screenshots\{VndbContentViewModel.VnId}\{Path.GetFileName(item.ImageLink)}";

                bool rating = NsfwHelper.RawRatingIsNsfw(item.Rating);
                bool userIsNsfw = NsfwHelper.UserIsNsfw(item.Rating);
                if (rating && File.Exists($"{thumbPath}.aes") && File.Exists($"{imagePath}.aes"))
                {
                    var imgBytes = File.ReadAllBytes($"{thumbPath}.aes");
                    var imgStream = Secure.DecStreamToStream(new MemoryStream(imgBytes));
                    image = ImageHelper.CreateBitmapFromStream(imgStream);
                    ScreenshotCollection.Add(CreateBlurBindingImage(image, userIsNsfw,blurWeight));
                }
                else if (rating == false && File.Exists(thumbPath) && File.Exists(imagePath))
                {
                    image = ImageHelper.CreateBitmapFromPath(thumbPath);
                    ScreenshotCollection.Add(CreateBlurBindingImage(image, userIsNsfw, blurWeight));
                }
                else
                {
                    toDelete.Add(item);

                }
            }

            foreach (var delete in toDelete)
            {
                _scrList.Remove(delete);
            }
        }

        private BindingImage CreateBlurBindingImage(BitmapSource image, bool isNsfw, int blurRadius)
        {
            if (isNsfw == false)
            {
                return new BindingImage { Image = image, IsNsfw = true };
            }
            else
            {
                var blurImage = ImageHelper.BlurImage(image, blurRadius);
                return new BindingImage { Image = blurImage, IsNsfw = true };
            }
        }

    }
}
