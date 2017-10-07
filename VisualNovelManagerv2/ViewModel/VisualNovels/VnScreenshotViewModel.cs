using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMain;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnScreenshotViewModel: ViewModelBase
    {       
        public VnScreenshotViewModel()
        {
        }

        public ICommand BindScreenshotsCommand => new RelayCommand(BindScreenshots);
        #region StaticProperties

        #region ScreenshotCollection
        private static ObservableCollection<ScreenshotViewModelCollection> _screenshotCollection = new ObservableCollection<ScreenshotViewModelCollection>();
        public ObservableCollection<ScreenshotViewModelCollection> ScreenshotCollection
        {
            get { return _screenshotCollection; }
            set
            {
                _screenshotCollection = value;
                RaisePropertyChanged(nameof(ScreenshotCollection));
            }
        }
        #endregion

        #region ScreenshotModel
        private VnScreenshotModel _screenshotModel= new VnScreenshotModel();
        public VnScreenshotModel ScreenshotModel
        {
            get { return _screenshotModel; }
            set
            {
                _screenshotModel = value;
                RaisePropertyChanged(nameof(ScreenshotModel));
            }
        }
        #endregion

        #region SelectedScreenIndex
        private int _selectedScreenIndex = -1;
        public int SelectedScreenIndex
        {
            get { return _selectedScreenIndex; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                _selectedScreenIndex = value;
                LoadLargeScreenshot();
                RaisePropertyChanged(nameof(SelectedScreenIndex));
            }
        }
        #endregion

        #region MainImage
        private BitmapImage _mainImage;
        public BitmapImage MainImage
        {
            get { return _mainImage; }
            set
            {
                _mainImage = value;
                RaisePropertyChanged(nameof(MainImage));
            }
        }
        #endregion

        public bool IsScreenshotDownloading = false;
        #endregion

        private static List<Screenshot> LoadScreenshotList()
        {
            try
            {
                List<Screenshot> screenshotList = new List<Screenshot>();
                using (var context = new DatabaseContext())
                {
                    foreach (VnInfoScreens screens in context.VnInfoScreens.Where(x=>x.VnId == Globals.VnId))
                    {
                        screenshotList.Add(new Screenshot
                        {
                            Url = screens.ImageUrl,
                            IsNsfw = Convert.ToBoolean(screens.Nsfw)
                        });
                    }
                }
                return screenshotList;
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }
            
        }
        
        private void BindScreenshots()
        {
            ScreenshotCollection.Clear();
            try
            {
                List<Screenshot> screenshotList = LoadScreenshotList();
                foreach (Screenshot screenshot in screenshotList)
                {
                    if (screenshotList.Count < 1) return;
                    string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileNameWithoutExtension(screenshot.Url)}";
                    string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}";

                    switch (screenshot.IsNsfw)
                    {
                        case true when File.Exists(pathNoExt):
                        {
                            BitmapImage bImage = Globals.NsfwEnabled ? Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt)) 
                                : new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\nsfw\thumb.jpg"));

                            _screenshotCollection.Add(new ScreenshotViewModelCollection
                            {
                                ScreenshotModel = new VnScreenshotModel {Screenshot = bImage}
                            });
                            break;
                        }
                        case false when File.Exists(path):
                        {
                            BitmapImage bitmap = new BitmapImage();
                            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = stream;
                                bitmap.EndInit();
                            }
                            _screenshotCollection.Add(new ScreenshotViewModelCollection
                            {
                                ScreenshotModel = new VnScreenshotModel {Screenshot = bitmap}
                            });
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }            
        }

        private void LoadLargeScreenshot()
        {
            try
            {
                List<Screenshot> screenshotList = LoadScreenshotList();
                if (screenshotList.Count <= 0) return;
                switch (screenshotList[SelectedScreenIndex].IsNsfw)
                {
                    case true:
                        if (Globals.NsfwEnabled == true)
                        {
                            string filename = Path.GetFileNameWithoutExtension(screenshotList[SelectedScreenIndex].Url);
                            string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{filename}";
                            BitmapImage bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));
                            MainImage = bImage;
                        }
                        else
                        {
                            MainImage = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\nsfw\screenshot.jpg"));
                        }
                        break;
                    case false:
                        string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{Path.GetFileName(screenshotList[SelectedScreenIndex].Url)}";
                        BitmapImage bitmap = new BitmapImage();
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            MainImage = bitmap;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }            
        }

        public async Task DonwloadScreenshotImagesPublic()
        {
            await DownloadScreenshots();
        }

        private async Task DownloadScreenshots()
        {
            if (ConnectionTest.VndbTcpSocketTest() == false)
            {
                Globals.Logger.Warn("Could not connect to Vndb API over SSL");
                Globals.StatusBar.SetOnlineStatusColor(false);
                Globals.StatusBar.IsShowOnlineStatusEnabled = true;
                await Task.Delay(3500);
                Globals.StatusBar.IsShowOnlineStatusEnabled = false;
                return;
            }
            try
            {
                IsScreenshotDownloading = true;
                Globals.StatusBar.IsWorkProcessing = true;
                Globals.StatusBar.ProgressText = "Downloading Screenshots";
                List<Screenshot> screenshotList = LoadScreenshotList();
                foreach (Screenshot screenshot in screenshotList)
                {
                    if (screenshotList.Count < 1) return;
                    if (!Directory.Exists($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs"))
                    {
                        Directory.CreateDirectory($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs");
                    }

                    string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{Path.GetFileNameWithoutExtension(screenshot.Url)}";
                    string pathNoExtThumb = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileNameWithoutExtension(screenshot.Url)}";
                    string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{Path.GetFileName(screenshot.Url)}";
                    string pathThumb = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}";
                    try
                    {
                        switch (screenshot.IsNsfw)
                        {
                            case true:
                                if (!File.Exists(pathNoExt))
                                {
                                    Globals.StatusBar.IsDownloading = true;
                                    WebClient client = new WebClient();
                                    using (MemoryStream stream = new MemoryStream(await client.DownloadDataTaskAsync(new Uri(screenshot.Url))))
                                    {

                                        string base64Img = Base64Converter.ImageToBase64(Image.FromStream(stream), ImageFormat.Jpeg);
                                        File.WriteAllText(pathNoExt, base64Img);                                    
                                    }
                                    client.Dispose();
                                }
                                if (!File.Exists(pathNoExtThumb))
                                {
                                    Globals.StatusBar.IsDownloading = true;
                                    WebClient client = new WebClient();
                                    using (MemoryStream stream = new MemoryStream(await client.DownloadDataTaskAsync(new Uri(screenshot.Url))))
                                    {
                                        var bitmap = new BitmapImage();
                                        bitmap.BeginInit();
                                        bitmap.StreamSource = stream;
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        bitmap.EndInit();
                                        bitmap.Freeze();
                                        Size thumbnailSize = GetThumbnailSize(bitmap);

                                        Image thumb = Image.FromStream(stream).GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, () => false, IntPtr.Zero);
                                        if (!File.Exists($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileNameWithoutExtension(screenshot.Url)}"))
                                        {
                                            File.WriteAllText(pathNoExtThumb, Base64Converter.ImageToBase64(thumb, ImageFormat.Jpeg));
                                        }
                                        thumb.Dispose();
                                    }
                                    client.Dispose();
                                }
                                Globals.StatusBar.IsDownloading = false;
                                break;
                            case false:
                                if (!File.Exists(path))
                                {
                                    Globals.StatusBar.IsDownloading = true;
                                    WebClient client = new WebClient();
                                    await client.DownloadFileTaskAsync(new Uri(screenshot.Url), path);
                                    client.Dispose();                               
                                }
                                if (!File.Exists(pathThumb))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        bitmap.BeginInit();
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        bitmap.StreamSource = stream;
                                        bitmap.EndInit();
                                        bitmap.Freeze();
                                    }
                                    Size thumnailSize = GetThumbnailSize(bitmap);
                                    Image thumb = Image.FromFile(path).GetThumbnailImage(thumnailSize.Width, thumnailSize.Height, () => false,IntPtr.Zero);

                                    if (!File.Exists($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}"))
                                    {
                                        thumb.Save($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}");
                                    }
                                    thumb.Dispose();
                                }
                                Globals.StatusBar.IsDownloading = false;
                                break;
                        }
                    }
                    catch (System.Net.WebException ex)
                    {
                        Globals.Logger.Error(ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error(ex);
                    }
                }
                Globals.StatusBar.ProgressText = string.Empty;
                Globals.StatusBar.IsWorkProcessing = false;
                IsScreenshotDownloading = false;
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
            
        }

        static Size GetThumbnailSize(BitmapImage original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 80;

            // Width and height.
            double originalWidth = original.Width;
            double originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

    }

    public class Screenshot
    {
        public string Url { get; set; }
        public bool IsNsfw { get; set; }
    }

    public class ScreenshotViewModelCollection
    {
        public VnScreenshotModel ScreenshotModel { get; set; }
    }
}
