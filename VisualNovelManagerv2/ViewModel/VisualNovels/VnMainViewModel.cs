using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
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
using VndbSharp;
using VndbSharp.Models.Dumps;


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
            VnInfoRelation = new ObservableCollection<VnInfoRelation>();
            VnInfoTagCollection = new ObservableCollection<string>();
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

        #region VnInfoRelation
        private ObservableCollection<VnInfoRelation> _vnInfoRelation;
        public ObservableCollection<VnInfoRelation> VnInfoRelation
        {
            get { return _vnInfoRelation; }
            set
            {
                _vnInfoRelation = value;
                RaisePropertyChanged(nameof(VnInfoRelation));
            }
        }
        #endregion

        #region VnTagObservableCollection
        private ObservableCollection<string> _vnInfoTagCollection;
        public ObservableCollection<string> VnInfoTagCollection
        {
            get { return _vnInfoTagCollection; }
            set
            {
                _vnInfoTagCollection = value;
                RaisePropertyChanged(nameof(VnInfoTagCollection));
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
            
            DataSet dataSet =new DataSet();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();

                #region OldConnection
                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT VnId FROM VnInfo WHERE PK_Id= @PK_Id";
                    cmd.Parameters.AddWithValue("@PK_Id", SelectedListItemIndex + 1);
                    Globals.VnId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                #endregion
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    var cmd =new SQLiteCommand("SELECT * FROM VnInfo WHERE PK_Id= @PK_Id", connection, transaction);
                    cmd.Parameters.AddWithValue("@PK_Id", SelectedListItemIndex + 1);
                    var cmd1 = new SQLiteCommand("SELECT * FROM VnInfoTags WHERE VnId=@VnId", connection, transaction);
                    cmd1.Parameters.AddWithValue("@VnId", Globals.VnId);
                    var cmd2 = new SQLiteCommand("SELECT * FROM VnInfoRelations WHERE VnId=@VnId", connection, transaction);
                    cmd2.Parameters.AddWithValue("@VnId", Globals.VnId);
                    var cmd3 = new SQLiteCommand("SELECT * FROM VnInfoAnime WHERE VnId=@VnId", connection, transaction);
                    cmd3.Parameters.AddWithValue("@VnId", Globals.VnId);
                    var cmd4 = new SQLiteCommand("SELECT * FROM VnInfoLinks WHERE VnId=@VnId", connection, transaction);
                    cmd4.Parameters.AddWithValue("@VnId", Globals.VnId);
                    var cmd5 = new SQLiteCommand("SELECT * FROM VnInfoStaff WHERE VnId=@VnId", connection, transaction);
                    cmd5.Parameters.AddWithValue("@VnId", Globals.VnId);
                    var cmd6 = new SQLiteCommand("SELECT * FROM VnInfoScreens WHERE VnId=@VnId", connection, transaction);
                    cmd6.Parameters.AddWithValue("@VnId", Globals.VnId);

                    dataSet.Tables.Add("VnInfo");
                    dataSet.Tables.Add("VnInfoTags");
                    dataSet.Tables.Add("VnInfoRelations");
                    dataSet.Tables.Add("VnInfoAnime");
                    dataSet.Tables.Add("VnInfoLinks");
                    dataSet.Tables.Add("VnInfoStaff");
                    dataSet.Tables.Add("VnInfoScreens");

                    adapter.SelectCommand = cmd;
                    adapter.Fill(dataSet.Tables["VnInfo"]);                    
                    adapter.SelectCommand = cmd1;
                    adapter.Fill(dataSet.Tables["VnInfoTags"]);
                    adapter.SelectCommand = cmd2;
                    adapter.Fill(dataSet.Tables["VnInfoRelations"]);
                    adapter.SelectCommand = cmd3;
                    adapter.Fill(dataSet.Tables["VnInfoAnime"]);
                    adapter.SelectCommand = cmd4;
                    adapter.Fill(dataSet.Tables["VnInfoLinks"]);
                    adapter.SelectCommand = cmd5;
                    adapter.Fill(dataSet.Tables["VnInfoStaff"]);
                    adapter.SelectCommand = cmd6;
                    adapter.Fill(dataSet.Tables["VnInfoScreens"]);

                    transaction.Commit();

                }
                connection.Close();
            }

            BindVnData(dataSet);
            
        }

        private async void BindVnData(DataSet dataSet)
        {
            _languageCollection.Clear();
            _originalLanguagesCollection.Clear();
            var vninfo = dataSet.Tables[0].Rows[0].ItemArray;

            DataTable dataTable = new DataTable();
            dataTable = dataSet.Tables["VnInfoRelations"];
            //TODO: WARNING: CURRENT BUILD OF VNDBSHARP ONLY RETURNS SEQUEL FOR RELATION TYPE
            foreach (DataRow row in dataTable.Rows)
            {
                _vnInfoRelation.Add(new VnInfoRelation { Title = row["Title"].ToString(), Original = row["Original"].ToString(), Relation = row["Relation"].ToString(), Official = row["Official"].ToString() });
            }

            //dataTable.Clear();
            dataTable = dataSet.Tables["VnInfoTags"];
            foreach (DataRow row in dataTable.Rows)
            {
                _vnInfoTagCollection.Add(row["TagId"].ToString());
            }

            VnMainModel.Name = vninfo[2].ToString();
            VnMainModel.VnIcon = LoadIcon();
            VnMainModel.Original = vninfo[3].ToString();
            VnMainModel.Released = vninfo[4].ToString();

            List<string> languages = GetLangauges(vninfo[5].ToString());
            foreach (string language in languages)
            {
                _languageCollection.Add(new LanguagesCollection { VnMainModel = new VnMainModel { Languages = new BitmapImage(new Uri(language)) } });
            }
            List<string> orig_languages = GetLangauges(vninfo[6].ToString());
            foreach (string language in orig_languages)
            {
                _originalLanguagesCollection.Add(new OriginalLanguagesCollection { VnMainModel = new VnMainModel { OriginalLanguages = new BitmapImage(new Uri(language)) } });
            }
            VnMainModel.Platforms = vninfo[7].ToString();
            VnMainModel.Aliases = vninfo[8].ToString();
            VnMainModel.Length = vninfo[9].ToString();
            VnMainModel.Description = ConvertRichTextDocument.ConvertToFlowDocument(vninfo[10].ToString());
            DownloadCoverImage(vninfo[11].ToString(), Convert.ToBoolean(vninfo[12]));
            VnMainModel.Popularity = Math.Round(Convert.ToDouble(vninfo[13]), 2);
            VnMainModel.Rating = Convert.ToInt32(vninfo[14]);

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

    public class VnInfoRelation
    {
        public string Title { get; set; }
        public string Relation { get; set; }
        public string Original { get; set; }
        public string Official { get; set; }
    }
}
