// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
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

        public BindableCollection<BindingImage> ScreenshotCollection { get; private set; } = new BindableCollection<BindingImage>();

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

        /// <summary>
        /// Get the list of images to bind to the ScreenSHotView
        /// </summary>
        /// <returns></returns>
        private List<BindingImage> LoadScreenshotList()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return new List<BindingImage>();
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'");
                var dbUserData = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString()).Query()
                    .Where(x => x.VnId == VndbContentViewModel.VnId).ToEnumerable();
                var scrList = dbUserData.Select(item => new BindingImage { ImageLink = item.ImageLink, Rating = item.ImageRating }).ToList();
                return scrList;
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to load Screenshot list Vndb");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                return new List<BindingImage>();
            }
        }

        /// <summary>
        /// Loads the full sized screenshot, blurring if it needs to
        /// </summary>
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
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }

        }

        /// <summary>
        /// Binds the collection of thumbnails to the Vndb Screenshot View
        /// </summary>
        private void BindScreenshotCollection()
        {
            try
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
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to bind Screenshot Collection");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }
        }

        /// <summary>
        /// Creates a blurred image if the image is Nsfw
        /// </summary>
        /// <param name="image"></param>
        /// <param name="isNsfw"></param>
        /// <param name="blurRadius"></param>
        /// <returns></returns>
        private BindingImage CreateBlurBindingImage(BitmapSource image, bool isNsfw, int blurRadius)
        {
            try
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
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to create BlurBinding image");
                SentryHelper.SendException(e, null, SentryLevel.Error);
                return new BindingImage();
            }
        }

    }
}
