using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.Design.VisualNovel;
using static System.Windows.FontStyles;
using Brushes = System.Windows.Media.Brushes;
using VisualNovelManagerv2.CustomClasses;


// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnMainViewModel : ViewModelBase
    {
        public ObservableCollection<string> VnNameCollection { get; set; }
        
        public ICommand BindVnNameCollectionCommand;
        public ICommand GetVnDataCommand { get; set; }
        public VnMainViewModel()
        {
            VnNameCollection = new ObservableCollection<string>();
            LanguageCollection = new ObservableCollection<LanguagesCollection>();
            OriginalLanguagesCollection = new ObservableCollection<OriginalLanguagesCollection>();
            BindVnNameCollectionCommand = new RelayCommand(LoadVnNameCollection);
            GetVnDataCommand = new RelayCommand(GetVnData);
            LoadVnNameCollection();
            _vnMainModel = new VnMainModel();
        }

        #region Static Properties

        #region maxwidth
        private double _maxListWidth;
        public double MaxListWidth
        {
            get { return _maxListWidth; }
            set
            {
                _maxListWidth = value;
                RaisePropertyChanged(nameof(MaxListWidth));
            }
        }
        #endregion

        #region SelectedListItemIndex
        private int _selectedListItemIndex;
        public int SelectedListItemIndex
        {
            get { return _selectedListItemIndex; }
            set
            {
                _selectedListItemIndex = value;
                RaisePropertyChanged(nameof(SelectedListItemIndex));
            }
        }
        #endregion

        #region VnMainMode
        private VnMainModel _vnMainModel;
        public VnMainModel VnMainModel
        {
            get { return _vnMainModel; }
            set
            {
                _vnMainModel = value;
                RaisePropertyChanged(nameof(VnMainModel));
            }
        }
        #endregion

        #region ObservableLanguageCollection
        private ObservableCollection<LanguagesCollection> _languageCollection;
        public ObservableCollection<LanguagesCollection> LanguageCollection
        {
            get { return _languageCollection; }
            set
            {
                _languageCollection = value;
                RaisePropertyChanged(nameof(LanguageCollection));
            }
        }
        #endregion

        #region ObserableOriginalLanguageCollection

        private ObservableCollection<OriginalLanguagesCollection> _originalLanguagesCollection;
        public ObservableCollection<OriginalLanguagesCollection> OriginalLanguagesCollection
        {
            get { return _originalLanguagesCollection; }
            set
            {
                _originalLanguagesCollection = value;
                RaisePropertyChanged(nameof(OriginalLanguagesCollection));
            }
        }
        

        #endregion


        #endregion
        private void LoadVnNameCollection()
        {
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = "SELECT Title FROM VnInfo";
                        VnNameCollection.Clear();
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            VnNameCollection.Add((string)reader["Title"]);
                        }
                    }

                connection.Close();
            }
            SetMaxWidth();

        }

        public void SetMaxWidth()
        {
            if (VnNameCollection.Count > 0)
            {
                string longestString = VnNameCollection.OrderByDescending(s => s.Length).First();
                MaxListWidth = MeasureStringSize.GetMaxStringWidth(longestString);
                Thread.Sleep(0);
            }
            
        }

        private void GetVnData()
        {
            _languageCollection.Clear();
            _originalLanguagesCollection.Clear();
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;

                        cmd.CommandText = "SELECT * FROM VnInfo WHERE PK_Id= @PK_Id";
                        cmd.Parameters.AddWithValue("@PK_Id", SelectedListItemIndex +1);
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        dataTable.Load(reader);
                        var items = dataTable.Rows[0].ItemArray;
                        Globals.VnId = Convert.ToInt32(items[1]);
                        VnMainModel.Name = items[2].ToString();
                        VnMainModel.VnIcon = LoadIcon();
                        VnMainModel.Original = items[3].ToString();
                        VnMainModel.Released = items[4].ToString();
                        VnMainModel.Platforms = items[7].ToString();
                        VnMainModel.Aliases = items[8].ToString();
                        VnMainModel.Length = items[9].ToString();
                        VnMainModel.Description = items[10].ToString();
                        DownloadCoverImage(items[11].ToString(), Convert.ToBoolean(items[12]));
                        VnMainModel.Popularity = Math.Round(Convert.ToDouble(items[13]), 2);
                        VnMainModel.Rating = Convert.ToInt32(items[14]);

                        
                        List<string> languages = GetLangauges(items[5].ToString());
                        foreach (string language in languages)
                        {
                            _languageCollection.Add(new LanguagesCollection { VnMainModel = new VnMainModel { Languages = new BitmapImage(new Uri(language)) } });
                        }

                        List<string> orig_languages = GetLangauges(items[6].ToString());
                        foreach (string language in orig_languages)
                        {
                            _originalLanguagesCollection.Add(new OriginalLanguagesCollection{VnMainModel = new VnMainModel{OriginalLanguages = new BitmapImage(new Uri(language)) } });
                        }
                        Thread.Sleep(0);




                        //TODO: put code to get all info I need in 1 transaction, so it runs fast.
                    }
                    transaction.Commit();
                }
                connection.Close();
            }
            VnScreenshotViewModel vm = new VnScreenshotViewModel();
            
            vm.DownloadScreenshots();
        }

        private List<string> GetLangauges(string csv)
        {
            List<string>filenames = new List<string>();
            var list = csv.Split(',');
            foreach (var lang in list)
            {
                switch (lang)
                {
                    case "ar":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "ca":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "cs":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "da":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "de":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "en":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "es":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "fi":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "fr":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "he":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "hr":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "hu":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "id":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "it":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "ja":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "ko":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "nl":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "pl":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "pt-br":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "pt-pt":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "ro":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "ru":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "sk":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "sv":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "ta":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "th":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "tr":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "uk":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "vi":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    case "zh":
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png");
                        break;
                    default:
                        filenames.Add($@"{Globals.DirectoryPath}\Data\res\country_flags\Unknown.png");
                        break;
                }
            }

            return filenames;
        }

        private BitmapSource LoadIcon()
        {
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM VnUserData WHERE VnId=@VnId";
                    cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("IconPath")))
                        {
                            string iconpath = (string)reader["IconPath"];
                            return CreateIcon(iconpath);
                        }
                        else if(reader.IsDBNull(reader.GetOrdinal("IconPath")))
                        {
                            string exepath = (string)reader["ExePath"];
                            return CreateIcon(exepath);
                        }
                        else
                        {
                            return CreateIcon(null);
                        }
                    }
                }                
            }
            return null;
        }

        private BitmapSource CreateIcon(string path)
        {
            if (path != null)
            {
                Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (sysicon == null)
                    return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] {0, 0, 0, 0}, 4);
                BitmapSource bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sysicon.Handle,System.Windows.Int32Rect.Empty,System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();
                return bmpSrc;
            }
            else
            {
                return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
            }
        }

        private void DownloadCoverImage(string url, bool nsfw)
        {
            string pathNoExt = string.Format(@"{0}\Data\images\cover\{1}", Globals.DirectoryPath, Globals.VnId);
            string path = string.Format(@"{0}\Data\images\cover\{1}.jpg", Globals.DirectoryPath, Globals.VnId);

            try
            {
                if (nsfw == true)
                {
                    if (!File.Exists(pathNoExt))
                    {
                        WebClient client = new WebClient();
                        using (MemoryStream stream = new MemoryStream(client.DownloadData(new Uri(url))))
                        {
                            string base64img =
                                Base64Converter.ImageToBase64(Image.FromStream(stream), ImageFormat.Jpeg);
                            File.WriteAllText(pathNoExt, base64img);
                        }
                    }
                }
                if (nsfw == false)
                {
                    if (!File.Exists(path))
                    {
                        WebClient client = new WebClient();
                        client.DownloadFile(new Uri(url), path);
                    }
                    
                }
                BindCoverImage(url, nsfw);
            }
            catch (WebException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        private void BindCoverImage(string url, bool? nsfw)
        {
            string pathNoExt = string.Format(@"{0}\Data\images\cover\{1}", Globals.DirectoryPath, Globals.VnId);
            string path = string.Format(@"{0}\Data\images\cover\{1}.jpg", Globals.DirectoryPath, Globals.VnId);

            try
            {
                if (nsfw == true)
                {
                    BitmapImage bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));
                    VnMainModel.Image = bImage;
                }
                if (nsfw == false)
                {
                    BitmapImage bImage = new BitmapImage(new Uri(path));
                    VnMainModel.Image = bImage;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }
    }

    public class LanguagesCollection
    {
        public VnMainModel VnMainModel { get; set; }
    }

    public class OriginalLanguagesCollection
    {
        public VnMainModel VnMainModel { get; set; }
    }
}
