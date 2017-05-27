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
        public ICommand GetScreenshotData { get; private set; }
        public ICommand LoadLargeScreenshotCommand { get; private set; }
        public static List<Screenshot> ScreenshotList = new List<Screenshot>();
        

        public VnScreenshotViewModel()
        {
            _screenshotModel = new VnScreenshotModel();
            GetScreenshotData = new RelayCommand(DownloadScreenshots);
            LoadLargeScreenshotCommand = new RelayCommand(LoadLargeScreenshot);
            
            ScreenshotCollection = new ObservableCollection<ScreenshotViewModelCollection>();
            Globals.VnId = 4;
            LoadScreenshotList();
            DownloadScreenshots();
        }

        #region StaticProperties
        private ObservableCollection<ScreenshotViewModelCollection> _screenshotCollection;
        public ObservableCollection<ScreenshotViewModelCollection> ScreenshotCollection
        {
            get { return _screenshotCollection; }
            set
            {
                _screenshotCollection = value;
                RaisePropertyChanged(nameof(ScreenshotCollection));
            }
        }

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

        private int _selectedScreenIndex = -1;

        public int SelectedScreenIndex
        {
            get { return _selectedScreenIndex; }
            set
            {
                //if (value < 0)
                //{
                //    value = 0;
                //}
                _selectedScreenIndex = value;
                RaisePropertyChanged(nameof(SelectedScreenIndex));
            }
        }


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


        private void LoadScreenshotList()
        {
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {

                    cmd.CommandText = "SELECT * FROM VnScreens WHERE VnId = @VnId ";
                    cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                    //ScreenshotList.Clear();
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ScreenshotList.Add(new Screenshot { Url = (string)reader["ImageUrl"], IsNsfw = Convert.ToBoolean(reader["Nsfw"]) });
                    }
                }
                connection.Close();
            }
        }

        public void DownloadScreenshots()
        {
            //TODO: check for no images in screenshots (some not popular vns have none)
            foreach (var screenshot in ScreenshotList)
            {
                if (!Directory.Exists(string.Format(@"{0}\Data\images\screenshots\{1}", Globals.DirectoryPath,Globals.VnId)))
                {
                    Directory.CreateDirectory(string.Format(@"{0}\Data\images\screenshots\{1}", Globals.DirectoryPath, Globals.VnId));
                }


                var image = screenshot.Url;
                string filename = Path.GetFileNameWithoutExtension(image);
                string pathNoExt = string.Format(@"{0}\Data\images\screenshots\{1}\{2}", Globals.DirectoryPath, Globals.VnId, filename);
                string path = string.Format(@"{0}\Data\images\screenshots\{1}\{2}", Globals.DirectoryPath, Globals.VnId, Path.GetFileName(image));
                if (screenshot.IsNsfw == true)
                {
                    if (!File.Exists(pathNoExt))
                    {
                        WebClient client = new WebClient();
                        using (MemoryStream stream = new MemoryStream(client.DownloadData(new Uri(image))))
                        {
                            string base64img = Base64Converter.ImageToBase64(Image.FromStream(stream), ImageFormat.Jpeg);
                            File.WriteAllText(pathNoExt, base64img);
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
            BindScreenshots();
        }

        private void BindScreenshots()
        {
            foreach (var screenshot in ScreenshotList)
            {
                var image = screenshot.Url;
                string filename = Path.GetFileNameWithoutExtension(image);
                string pathNoExt = string.Format(@"{0}\Data\images\screenshots\{1}\{2}", Globals.DirectoryPath, Globals.VnId, filename);
                string path = string.Format(@"{0}\Data\images\screenshots\{1}\{2}", Globals.DirectoryPath, Globals.VnId, Path.GetFileName(image));

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

        private void LoadLargeScreenshot()
        {

            if (ScreenshotList[SelectedScreenIndex].IsNsfw == true)
            {
                string filename = Path.GetFileNameWithoutExtension(ScreenshotList[SelectedScreenIndex].Url);
                string pathNoExt = string.Format(@"{0}\Data\images\screenshots\{1}\{2}", Globals.DirectoryPath, Globals.VnId, filename);
                BitmapImage bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));
                MainImage = bImage;
            }
            if (ScreenshotList[SelectedScreenIndex].IsNsfw == false)
            {
                string path = string.Format(@"{0}\Data\images\screenshots\{1}\{2}", Globals.DirectoryPath, Globals.VnId, Path.GetFileName(ScreenshotList[SelectedScreenIndex].Url));
                BitmapImage bImage = new BitmapImage(new Uri(path));
                MainImage = bImage;;
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
