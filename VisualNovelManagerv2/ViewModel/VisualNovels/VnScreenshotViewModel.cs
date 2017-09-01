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
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnScreenshotViewModel: ViewModelBase
    {       
        public VnScreenshotViewModel()
        {
            _screenshotModel = new VnScreenshotModel();
            
            //ScreenshotCollection = new ObservableCollection<ScreenshotViewModelCollection>();
            BindScreenshots();
        }

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
        private VnScreenshotModel _screenshotModel;
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

        #endregion

        private static List<Screenshot> LoadScreenshotList()
        {
            try
            {
                List<Screenshot> screenshotList = new List<Screenshot>();
                using (var db = new DatabaseContext())
                {
                    foreach (VnInfoScreens screens in db.Set<VnInfoScreens>().Where(x=>x.VnId == Globals.VnId))
                    {
                        screenshotList.Add(new Screenshot
                        {
                            Url = screens.ImageUrl,
                            IsNsfw = Convert.ToBoolean(screens.Nsfw)
                        });
                    }
                    db.Dispose();
                }
                return screenshotList;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }
        
        private void BindScreenshots()
        {
            ScreenshotCollection.Clear();
            ViewModelLocator.CleanupScreenshotViewModel();
            try
            {
                List<Screenshot> screenshotList = LoadScreenshotList();
                foreach (Screenshot screenshot in screenshotList)
                {
                    if (screenshotList.Count < 1) return;
                    string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileNameWithoutExtension(screenshot.Url)}";
                    string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}";

                    if (screenshot.IsNsfw == true && File.Exists(pathNoExt))
                    {
                        BitmapImage bImage = new BitmapImage();
                        if (Globals.NsfwEnabled == true)
                        {
                            bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));
                        }
                        else
                        {
                            bImage = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\nsfw\thumb.jpg"));
                        }
                        

                        _screenshotCollection.Add(new ScreenshotViewModelCollection
                        {
                            ScreenshotModel = new VnScreenshotModel {Screenshot = bImage}
                        });
                    }
                    if (screenshot.IsNsfw == false && File.Exists(path))
                    {
                        BitmapImage bImage = new BitmapImage(new Uri(path));
                        _screenshotCollection.Add(new ScreenshotViewModelCollection
                        {
                            ScreenshotModel = new VnScreenshotModel {Screenshot = bImage}
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        private void LoadLargeScreenshot()
        {
            try
            {
                List<Screenshot> screenshotList = LoadScreenshotList();
                if (screenshotList[SelectedScreenIndex].IsNsfw == true)
                {
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
                    
                }
                if (screenshotList[SelectedScreenIndex].IsNsfw == false)
                {
                    string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{
                            Path.GetFileName(screenshotList[SelectedScreenIndex].Url)
                        }";
                    BitmapImage bImage = new BitmapImage(new Uri(path));
                    MainImage = bImage; ;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        public static void DownloadScreenshots()
        {
            try
            {
                Globals.StatusBar.IsWorkProcessing = true;
                Globals.StatusBar.ProgressText = "Downloading Screenshots";
                VnMainViewModel.IsDownloading = true;
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
                        if (screenshot.IsNsfw == true)
                        {
                            if (!File.Exists(pathNoExt))
                            {
                                Globals.StatusBar.IsDownloading = true;
                                WebClient client = new WebClient();
                                using (MemoryStream stream = new MemoryStream(client.DownloadData(new Uri(screenshot.Url))))
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
                                using (MemoryStream stream = new MemoryStream(client.DownloadData(new Uri(screenshot.Url))))
                                {
                                    //write thumbnail
                                    while (client.IsBusy)
                                    {
                                        Thread.Sleep(100);
                                    }

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
                        }
                        if (screenshot.IsNsfw == false)
                        {
                            if (!File.Exists(path))
                            {
                                Globals.StatusBar.IsDownloading = true;
                                WebClient client = new WebClient();
                                client.DownloadFile(new Uri(screenshot.Url), path);
                                client.Dispose();                               
                            }
                            if (!File.Exists(pathThumb))
                            {
                                Size thumnailSize = GetThumbnailSize(new BitmapImage(new Uri(path)));
                                Image thumb = Image.FromFile(path).GetThumbnailImage(thumnailSize.Width, thumnailSize.Height, () => false,IntPtr.Zero);

                                if (!File.Exists($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}"))
                                {
                                    thumb.Save($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\thumbs\{Path.GetFileName(screenshot.Url)}");
                                }
                                thumb.Dispose();
                            }
                            Globals.StatusBar.IsDownloading = false;
                        }
                    }
                    catch (System.Net.WebException ex)
                    {
                        DebugLogging.WriteDebugLog(ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.WriteDebugLog(ex);
                    }
                }
                VnMainViewModel.IsDownloading = false;
                Globals.StatusBar.IsWorkProcessing = false;
                Globals.StatusBar.ProgressText = string.Empty;
            }
            catch (Exception exception)
            {
                DebugLogging.WriteDebugLog(exception);
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
