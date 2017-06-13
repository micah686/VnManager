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
using System.Windows.Documents;
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
        #region observableCollections

        #region ObservableVnNameCollection
        private ObservableCollection<string> _vnNameCollection = new ObservableCollection<string>();
        public ObservableCollection<string> VnNameCollection
        {
            get { return _vnNameCollection; }
            set
            {
                _vnNameCollection = value;
                RaisePropertyChanged(nameof(VnNameCollection));
            }
        }
        #endregion

        #region ObservableVnInfoAnime
        private ObservableCollection<VnInfoAnime> _vnAnimeCollection = new ObservableCollection<VnInfoAnime>();
        public ObservableCollection<VnInfoAnime> VnInfoAnimeCollection
        {
            get { return _vnAnimeCollection; }
            set
            {
                _vnAnimeCollection = value;
                RaisePropertyChanged(nameof(VnInfoAnimeCollection));
            }
        }
        #endregion

        #region ObservableLanguageCollection
        private ObservableCollection<LanguagesCollection> _languageCollection = new ObservableCollection<LanguagesCollection>();
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
        private ObservableCollection<OriginalLanguagesCollection> _originalLanguagesCollection = new ObservableCollection<OriginalLanguagesCollection>();
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
        private ObservableCollection<VnInfoRelation> _vnInfoRelation = new ObservableCollection<VnInfoRelation>();
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

        #region ObservableCollectionVnTag
        private ObservableCollection<string> _vnInfoTagCollection = new ObservableCollection<string>();
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

        public static readonly VnCharacterViewModel VnCharacterViewModel = new VnCharacterViewModel();



        public static ICommand LoadBindVnDataCommand { get; set; }
        public static ICommand ClearCollectionsCommand { get; private set; }
        public VnMainViewModel()
        {
            LoadBindVnDataCommand = new RelayCommand(LoadVnNameCollection);
            ClearCollectionsCommand = new RelayCommand(ClearCollections);

            _vnMainModel = new VnMainModel();
            LoadVnNameCollection();
            
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

        #region VnMainModel
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

        #region SelectedVn
        private string _selectedVn;
        public string SelectedVn
        {
            get { return _selectedVn; }
            set
            {
                _selectedVn = value;
                RaisePropertyChanged(nameof(SelectedVn));
                GetVnData();
            }
        }
        #endregion

        #region SelectedTag
        private string _selectedTag;
        public string SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                _selectedTag = value;
                RaisePropertyChanged(nameof(SelectedTag));
                BindTagDescription();
            }
        }
        #endregion

        #region SelectedTagIndex
        private int? _selectedTagIndex;
        public int? SelectedTagIndex
        {
            get { return _selectedTagIndex; }
            set
            {
                _selectedTagIndex = value;
                RaisePropertyChanged(nameof(SelectedTagIndex));
            }
        }        
        #endregion

        #region TagDescription
        private FlowDocument _tagDescription;
        public FlowDocument TagDescription
        {
            get { return _tagDescription; }
            set
            {
                _tagDescription = value;
                RaisePropertyChanged(nameof(TagDescription));
            }
        }        
        #endregion

        #endregion

        private void ClearCollections()
        {
            VnNameCollection.Clear();
            LanguageCollection.Clear();
            OriginalLanguagesCollection.Clear();
            VnInfoRelation.Clear();
            VnInfoTagCollection.Clear();
            VnInfoAnimeCollection.Clear();
        }

        private void LoadVnNameCollection()
        {
            try
            {
                ObservableCollection<string> nameList = new ObservableCollection<string>();
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Title FROM VnInfo";
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            nameList.Add((string)reader["Title"]);
                        }
                    }

                    connection.Close();
                }
                VnNameCollection = nameList;
                SetMaxWidth();
            }
            catch (SQLiteException ex)
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

        public void SetMaxWidth()
        {
            if (VnNameCollection.Count <= 0) return;
            string longestString = VnNameCollection.OrderByDescending(s => s.Length).First();
            MaxListWidth = MeasureStringSize.GetMaxStringWidth(longestString);
            Thread.Sleep(0);
        }

        private void GetVnData()
        {
            try
            {
                LanguageCollection.Clear();
                OriginalLanguagesCollection.Clear();
                VnInfoRelation.Clear();
                VnInfoTagCollection.Clear();
                VnInfoAnimeCollection.Clear();
                _tagDescription?.Blocks.Clear();
                DataSet dataSet = new DataSet();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();

                    #region GetVnId
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT VnId FROM VnInfo WHERE PK_Id= @PK_Id";
                        cmd.Parameters.AddWithValue("@PK_Id", SelectedListItemIndex + 1);
                        Globals.VnId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    #endregion

                    #region SQLite Transaction
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM VnInfo WHERE PK_Id= @PK_Id", connection, transaction);
                        cmd.Parameters.AddWithValue("@PK_Id", SelectedListItemIndex + 1);
                        SQLiteCommand cmd1 = new SQLiteCommand("SELECT * FROM VnInfoTags WHERE VnId=@VnId", connection, transaction);
                        cmd1.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteCommand cmd2 = new SQLiteCommand("SELECT * FROM VnInfoRelations WHERE VnId=@VnId", connection, transaction);
                        cmd2.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteCommand cmd3 = new SQLiteCommand("SELECT * FROM VnInfoAnime WHERE VnId=@VnId", connection, transaction);
                        cmd3.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteCommand cmd4 = new SQLiteCommand("SELECT * FROM VnInfoLinks WHERE VnId=@VnId", connection, transaction);
                        cmd4.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteCommand cmd5 = new SQLiteCommand("SELECT * FROM VnInfoStaff WHERE VnId=@VnId", connection, transaction);
                        cmd5.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteCommand cmd6 = new SQLiteCommand("SELECT * FROM VnInfoScreens WHERE VnId=@VnId", connection, transaction);
                        cmd6.Parameters.AddWithValue("@VnId", Globals.VnId);
                        SQLiteCommand cmd7 = new SQLiteCommand("SELECT * FROM VnUserData WHERE VnId=@VnId", connection, transaction);
                        cmd7.Parameters.AddWithValue("@VnId", Globals.VnId);

                        dataSet.Tables.Add("VnInfo");
                        dataSet.Tables.Add("VnInfoTags");
                        dataSet.Tables.Add("VnInfoRelations");
                        dataSet.Tables.Add("VnInfoAnime");
                        dataSet.Tables.Add("VnInfoLinks");
                        dataSet.Tables.Add("VnInfoStaff");
                        dataSet.Tables.Add("VnInfoScreens");
                        dataSet.Tables.Add("VnUserData");

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
                        adapter.SelectCommand = cmd7;
                        adapter.Fill(dataSet.Tables["VnUserData"]);

                        transaction.Commit();
                    }
                    #endregion

                    connection.Close();
                }
                BindVnData(dataSet);
            }
            catch (SQLiteException ex)
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

        private void BindVnData(DataSet dataSet)
        {
            try
            {
                object[] vninfo = dataSet.Tables[0].Rows[0].ItemArray;
                DataTable dataTable = new DataTable();
                dataTable = dataSet.Tables["VnInfoRelations"];
                foreach (DataRow row in dataTable.Rows)
                {
                    _vnInfoRelation.Add(new VnInfoRelation { Title = row["Title"].ToString(), Original = row["Original"].ToString(), Relation = row["Relation"].ToString(), Official = row["Official"].ToString() });
                }

                dataTable = dataSet.Tables["VnInfoTags"];
                foreach (DataRow row in dataTable.Rows)
                {
                    _vnInfoTagCollection.Add(row["TagName"].ToString());
                }

                dataTable = dataSet.Tables["VnInfoAnime"];
                foreach (DataRow row in dataTable.Rows)
                {
                    object row2 = row.ItemArray[2];
                    object row3 = row.ItemArray[3];
                    string anidb = null;
                    string ann = null;
                    if (row2 != null)
                    {
                        anidb = $"anidb.net/a{row.ItemArray[2].ToString()}";
                    }
                    if (row3 != null)
                    {
                        ann = $"animenewsnetwork.com/encyclopedia/anime.php?id={row.ItemArray[2].ToString()}";
                    }
                    _vnAnimeCollection.Add(new VnInfoAnime
                    {
                        Title = row["TitleEng"].ToString(),
                        OriginalName = row["TitleJpn"].ToString(),
                        Year = row["Year"].ToString(),
                        AnimeType = row["AnimeType"].ToString(),
                        AniDb = anidb,
                        Ann = ann
                    });
                }
                VnMainModel.Name = vninfo[2].ToString();
                VnMainModel.VnIcon = LoadIcon();
                VnMainModel.Original = vninfo[3].ToString();
                VnMainModel.Released = vninfo[4].ToString();

                IEnumerable<string> languages = GetLangauges(vninfo[5].ToString());
                foreach (string language in languages)
                {
                    _languageCollection.Add(new LanguagesCollection { VnMainModel = new VnMainModel { Languages = new BitmapImage(new Uri(language)) } });
                }
                IEnumerable<string> origLanguages = GetLangauges(vninfo[6].ToString());
                foreach (string language in origLanguages)
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

                #region UserData Bind
                dataTable = dataSet.Tables["VnUserData"];
                object[] vnuserdata = dataSet.Tables[7].Rows[0].ItemArray;

                if (vnuserdata[4].ToString() == "")
                {
                    VnMainModel.LastPlayed = "Never";
                }
                else
                {
                    if ((Convert.ToDateTime(vnuserdata[4]) - DateTime.Today).Days > -7)//need to set to negative, for the difference in days
                    {
                        if (Convert.ToDateTime(vnuserdata[4]) == DateTime.Today)
                        {
                            VnMainModel.LastPlayed = "Today";
                        }
                        else if ((Convert.ToDateTime(vnuserdata[4]) - DateTime.Today).Days > -2 && (Convert.ToDateTime(vnuserdata[4]) - DateTime.Today).Days < 0)
                        {
                            VnMainModel.LastPlayed = "Yesterday";
                        }
                        else
                        {
                            VnMainModel.LastPlayed = Convert.ToDateTime(vnuserdata[4]).DayOfWeek.ToString();
                        }
                    }
                    else
                    {
                        VnMainModel.LastPlayed = vnuserdata[4].ToString();
                    }
                }

                string[] splitPlayTime = vnuserdata[5].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> timeList = new List<int>(4);
                timeList.AddRange(splitPlayTime.Select(time => Convert.ToInt32(time)));
                TimeSpan timeSpan = new TimeSpan(timeList[0], timeList[1], timeList[2], timeList[3]);

                if (timeSpan < new TimeSpan(0, 0, 0, 1))
                {
                    VnMainModel.PlayTime = "Never";
                }
                if (timeSpan < new TimeSpan(0, 0, 0, 60))
                {
                    VnMainModel.PlayTime = "Less than 1 minute";
                }
                else
                {
                    string formatted = $"{(timeSpan.Duration().Days > 0 ? $"{timeSpan.Days:0} day{(timeSpan.Days == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                                       $"{(timeSpan.Duration().Hours > 0 ? $"{timeSpan.Hours:0} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                                       $"{(timeSpan.Duration().Minutes > 0 ? $"{timeSpan.Minutes:0} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")} " : string.Empty)}";
                    VnMainModel.PlayTime = formatted;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
            VnScreenshotViewModel vm = new VnScreenshotViewModel();
            vm.DownloadScreenshots();

            VnReleaseViewModel vmrel = new VnReleaseViewModel();
            vmrel.LoadReleaseCommand.Execute(null);

            VnCharacterViewModel vmchar = new VnCharacterViewModel();
            vmchar.LoadCharacterCommand.Execute(null);
        }



        private void BindTagDescription()
        {
            try
            {
                if (!(_selectedTagIndex >= 0)) return;
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Description FROM VnTagData WHERE Name= @Name";
                        cmd.Parameters.AddWithValue("@Name", SelectedTag);

                        TagDescription = ConvertRichTextDocument.ConvertToFlowDocument(cmd.ExecuteScalar().ToString());
                    }
                }
            }
            catch (SQLiteException ex)
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
        
        private static IEnumerable<string> GetLangauges(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\country_flags\{lang}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\country_flags\Unknown.png")
                .ToList();
        }

        private BitmapSource LoadIcon()
        {
            try
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
                            else if (reader.IsDBNull(reader.GetOrdinal("IconPath")))
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
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

        private BitmapSource CreateIcon(string path)
        {
            try
            {
                if (path == null)
                {
                    return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
                }
                Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (sysicon == null)
                    return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
                BitmapSource bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sysicon.Handle, System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();
                return bmpSrc;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }

        private void DownloadCoverImage(string url, bool nsfw)
        {
            string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}";
            string path = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg";

            try
            {
                if (nsfw == true)
                {
                    if (!File.Exists(pathNoExt))
                    {
                        WebClient client = new WebClient();
                        using (MemoryStream stream = new MemoryStream(client.DownloadData(new Uri(url))))
                        {
                            string base64Img =
                                Base64Converter.ImageToBase64(Image.FromStream(stream), ImageFormat.Jpeg);
                            File.WriteAllText(pathNoExt, base64Img);
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
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private void BindCoverImage(string url, bool? nsfw)
        {
            string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}";
            string path = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg";

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

    public class VnInfoAnime
    {
        public string Title { get; set; }
        public string OriginalName { get; set; }
        public string Year { get; set; }
        public string AnimeType { get; set; }
        public string AniDb { get; set; }
        public string Ann { get; set; }
    }
}
