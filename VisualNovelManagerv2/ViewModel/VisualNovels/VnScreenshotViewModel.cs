using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using System.Drawing.Drawing2D;
using System.Windows.Input;
using System.Data.SQLite;
using System.Drawing.Imaging;
using System.IO;
using GalaSoft.MvvmLight.CommandWpf;
using System.Net;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.Design.VisualNovel;

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
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM VnInfoScreens WHERE VnId = @VnId ";
                        cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            screenshotList.Add(new Screenshot
                            {
                                Url = (string) reader["ImageUrl"],
                                IsNsfw = Convert.ToBoolean(reader["Nsfw"])
                            });
                        }
                    }
                    connection.Close();
                }
                return screenshotList;
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
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
            try
            {
                List<Screenshot> screenshotList = LoadScreenshotList();
                foreach (Screenshot screenshot in screenshotList)
                {
                    if (screenshotList.Count < 1) return;
                    string image = screenshot.Url;
                    string filename = Path.GetFileNameWithoutExtension(image);
                    string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{filename}";
                    string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{Path.GetFileName(image)}";

                    if (screenshot.IsNsfw == true)
                    {
                        BitmapImage bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));

                        _screenshotCollection.Add(new ScreenshotViewModelCollection
                        {
                            ScreenshotModel = new VnScreenshotModel { Screenshot = bImage }
                        });
                    }
                    if (screenshot.IsNsfw == false)
                    {
                        BitmapImage bImage = new BitmapImage(new Uri(path));
                        _screenshotCollection.Add(new ScreenshotViewModelCollection
                        {
                            ScreenshotModel = new VnScreenshotModel { Screenshot = bImage }
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
                    string filename = Path.GetFileNameWithoutExtension(screenshotList[SelectedScreenIndex].Url);
                    string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{filename}";
                    BitmapImage bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));
                    MainImage = bImage;
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
            List<Screenshot> screenshotList = LoadScreenshotList();
            foreach (Screenshot screenshot in screenshotList)
            {
                if (screenshotList.Count < 1) return;
                if (!Directory.Exists($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}"))
                {
                    Directory.CreateDirectory($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}");
                }

                string image = screenshot.Url;
                string filename = Path.GetFileNameWithoutExtension(image);
                string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{filename}";
                string path = $@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}\{Path.GetFileName(image)}";
                try
                {
                    if (screenshot.IsNsfw == true)
                    {
                        if (!File.Exists(pathNoExt))
                        {
                            WebClient client = new WebClient();
                            using (MemoryStream stream = new MemoryStream(client.DownloadData(new Uri(image))))
                            {
                                string base64Img =
                                    Base64Converter.ImageToBase64(Image.FromStream(stream), ImageFormat.Jpeg);
                                File.WriteAllText(pathNoExt, base64Img);
                            }
                        }
                    }
                    if (screenshot.IsNsfw == false)
                    {
                        if (!File.Exists(path))
                        {
                            WebClient client = new WebClient();
                            client.DownloadFile(new Uri(image), path);
                        }
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
