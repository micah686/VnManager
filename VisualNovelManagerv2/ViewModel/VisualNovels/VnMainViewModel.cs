using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.EntityFrameworkCore;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using Image = System.Drawing.Image;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;


// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public partial class VnMainViewModel : ViewModelBase
    {
        public static ICommand LoadBindVnDataCommand { get; private set; }
        public static ICommand ClearCollectionsCommand { get; private set; }
        public VnMainViewModel()
        {
            LoadBindVnDataCommand = new RelayCommand(LoadCategories);
            ClearCollectionsCommand = new RelayCommand(ClearCollections);
            AddToCategoryCommand = new RelayCommand<string>(AddToCategory);
            RemoveFromCategoryCommand = new RelayCommand<string>(RemoveFromCategory);
            _vnMainModel = new VnMainModel();
            LoadCategories();
            
        }

        

        private void ClearCollections()
        {
            VnNameCollection.Clear();
            LanguageCollection.Clear();
            OriginalLanguagesCollection.Clear();
            VnInfoRelation.Clear();
            VnInfoTagCollection.Clear();
            VnInfoAnimeCollection.Clear();
        }

        private void LoadCategories()
        {
            VnNameCollection.Clear();
            try
            {
                using (var context = new DatabaseContext())
                {
                    //need to add/seed the "All" in StartupValidate
                    if (_selectedCategory == "All" || string.IsNullOrEmpty(_selectedCategory))
                    {
                        if (context.VnInfo != null)
                        {
                            VnNameCollection.InsertRange(context.VnInfo.Select(x => x.Title).ToList());
                            return;
                        }
                    }                    
                    VnNameCollection.InsertRange(context.Categories.Where(cn => cn.CategoryName == _selectedCategory)
                        .SelectMany(x => x.CategoryJunctions.Select(y => y.VnUserCategoryTitle)).Select(z => z.Title));                    
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            finally
            {
                SetMaxWidth();
            }
            
        }


        private void LoadAddRemoveCategoryWindow()
        {
            Messenger.Default.Send(new NotificationMessage("Show Add/Remove Category Window"));
        }
        

        public void SetMaxWidth()
        {
            if (VnNameCollection.Count <= 0) return;
            string longestString = VnNameCollection.OrderByDescending(s => s.Length).First();
            MaxListWidth = MeasureStringSize.GetMaxStringWidth(longestString);
        }

        private async void GetVnData()
        {
            while (IsDownloading== true)
            {
                await Task.Delay(100);
            }
            if (IsDownloading == false)
            {
                try
                {
                    LanguageCollection.Clear();
                    PlatformCollection.Clear();
                    OriginalLanguagesCollection.Clear();
                    VnInfoRelation.Clear();
                    VnInfoTagCollection.Clear();
                    VnInfoAnimeCollection.Clear();
                    _tagDescription?.Blocks.Clear();

                    using (var db = new DatabaseContext())
                    {
                        foreach (int vnid in db.Set<VnInfo>().Where(t=>t.Title==(SelectedVn)).Select(v=>v.VnId))
                        {
                            Globals.VnId = vnid;
                        }
                        db.Dispose();
                    }

                    await Task.Run((BindVnData));
                    await Task.Run((() => VnScreenshotViewModel.DownloadScreenshots()));
                    UpdateViews();
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    throw;
                }
            }
            
        }

        

        private void UpdateViews()
        {
            new VnCharacterViewModel();
            new VnScreenshotViewModel();
            new VnReleaseViewModel();
        }

        
        
        private static IEnumerable<string> GetLangauges(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\Unknown.png")
                .ToList();
        }

        private static IEnumerable<string> GetPlatforms(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(plat => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\platforms\{plat}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\platforms\{plat}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\platforms\Unknown.png")
                .ToList();
        }

        private BitmapSource LoadIcon()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    foreach (VnUserData userData in db.Set<VnUserData>().Where(v=>v.VnId == Globals.VnId))
                    {
                        //checks for existance of Iconpath first, then ExePath. If both are null/empty, it returns a null image
                        return !string.IsNullOrEmpty(userData.IconPath) ? CreateIcon(userData.IconPath) : CreateIcon(!string.IsNullOrEmpty(userData.ExePath) ? userData.ExePath : null);
                    }
                    db.Dispose();
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

        private async void DownloadCoverImage(string url, bool nsfw)
        {
            string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}";
            string path = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg";

            try
            {
                if (nsfw == true)
                {
                    if (!File.Exists(pathNoExt))
                    {
                        Globals.StatusBar.IsDownloading = true;
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
                        Globals.StatusBar.IsDownloading = true;
                        Thread.Sleep(1500);
                        WebClient client = new WebClient();
                        client.DownloadFile(new Uri(url), path);
                    }
                    
                }
                Globals.StatusBar.IsDownloading = false;
                await Application.Current.Dispatcher.BeginInvoke(new Action((() => BindCoverImage(url, nsfw))));
                //BindCoverImage(url, nsfw);
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

        private void StartVn()
        {
            _stopwatch.Reset();
            Process process = null;
            using (var context = new DatabaseContext())
            {
                VnInfo idList = context.VnInfo.FirstOrDefault(x => x.Title.Equals(SelectedVn));
                VnUserData vnUserData = context.VnUserData.FirstOrDefault(x => x.VnId.Equals(idList.VnId));
                if (vnUserData?.ExePath != null)
                {
                    string exepath = vnUserData.ExePath;
                    string dirpath = Path.GetDirectoryName(exepath);
                    if (dirpath != null) Directory.SetCurrentDirectory(dirpath);
                    process = Process.Start(exepath);
                    _stopwatch.Start();
                }
            }
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
            List<Process> children = process.GetChildProcesses();
            int initialChildrenCount = children.Count;
            if (process != null)
            {
                process.EnableRaisingEvents = true;
                process.Exited += delegate(object o, EventArgs args)
                {
                    //Only allows the HasExited event to trigger when there are no child processes
                    if (children.Count < 1 && initialChildrenCount == 0)
                    {
                        _stopwatch.Stop();
                        VnOrChildProcessExited(null, null);
                    }
                };
            }

            //this may break if the parent spawns multiple child processes.
            //TODO: check against programs that spawn multiple processes, like chrome
            foreach (Process proc in children)
            {
                proc.EnableRaisingEvents = true;
                proc.Exited += VnOrChildProcessExited;
            }

        }

        private void VnOrChildProcessExited(object sender, EventArgs eventArgs)
        {
            try
            {
                _stopwatch.Stop();
                VnUserData vnUserData;
                using (var context = new DatabaseContext())
                {
                    vnUserData = context.VnUserData.FirstOrDefault(x => x.VnId.Equals(Convert.ToUInt32(Globals.VnId)));
                }

                if (vnUserData != null)
                {
                    vnUserData.LastPlayed = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(vnUserData.PlayTime))
                    {
                        //check if matches #,#,#,# format
                        bool isMatch = new Regex(@"^[0-9]+\,[0-9]+\,[0-9]+\,[0-9]+$").IsMatch(vnUserData.PlayTime);
                        if (new Regex(@"^[0-9]+\,[0-9]+\,[0-9]+\,[0-9]+$").IsMatch(vnUserData.PlayTime))
                        {
                            var lastPlayTime = vnUserData.PlayTime.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            List<int> timecount = new List<int>();
                            for (int i = 0; i < lastPlayTime.Length; i++)
                            {
                                timecount.Add(new int());
                                timecount[i] = Convert.ToInt32(lastPlayTime[i]);
                            }
                            TimeSpan timeSpan = new TimeSpan(timecount[0], timecount[1], timecount[2], timecount[3]);
                            TimeSpan currentplaytime = new TimeSpan(_stopwatch.Elapsed.Days, _stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds);
                            timeSpan = timeSpan.Add(currentplaytime);
                            vnUserData.PlayTime = $"{timeSpan.Days},{timeSpan.Hours},{timeSpan.Minutes},{timeSpan.Seconds}";
                        }                        
                    }
                }

                using (var context = new DatabaseContext())
                {
                    if (vnUserData == null) return;
                    context.Entry(vnUserData).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }
    }

    //class for bindings
    public partial class VnMainViewModel
    {
        private void BindCoverImage(string url, bool? nsfw)
        {
            string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}";
            string path = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg";

            try
            {
                switch (nsfw)
                {
                    case true:
                    {
                        BitmapImage bImage = Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt));
                        VnMainModel.Image = bImage;
                        break;
                    }
                    case false:
                    {
                        BitmapImage bImage = new BitmapImage(new Uri(path));
                        VnMainModel.Image = bImage;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private void BindTagDescription()
        {
            try
            {
                if (!(_selectedTagIndex >= 0)) return;
                using (var db = new DatabaseContext())
                {
                    foreach (string tag in db.Set<VnTagData>().Where(n => n.Name == SelectedTag).Select(d => d.Description))
                    {
                        TagDescription = ConvertRichTextDocument.ConvertToFlowDocument(tag);
                        break;
                    }
                    db.Dispose();
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private async Task BindVnData()
        {
            double ProgressIncrement = 11.11111111111111;
            Globals.StatusBar.ProgressPercentage = 0;
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.ProgressText = "Loading Main Data";
            try
            {
                using (var db = new DatabaseContext())
                {


                    #region VnInfo
                    foreach (VnInfo vnInfo in db.Set<VnInfo>().Where(t => t.Title == (SelectedVn)))
                    {
                        Globals.VnId = Convert.ToInt32(vnInfo.VnId);
                        //TODO:Change Globals.VnId To uint


                        foreach (string language in GetLangauges(vnInfo.Languages))
                        {
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => LanguageCollection.Add(
                                new LanguagesCollection
                                    { VnMainModel = new VnMainModel { Languages = new BitmapImage(new Uri(language)) } })));
                        }

                        foreach (string platform in GetPlatforms(vnInfo.Platforms))
                        {
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => PlatformCollection.Add(
                                new PlatformCollection { VnMainModel = new VnMainModel { Platforms = new BitmapImage(new Uri(platform)) } })));
                        }

                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        foreach (string language in GetLangauges(vnInfo.OriginalLanguage))
                        {
                            await Application.Current.Dispatcher.BeginInvoke(new Action(
                                () => OriginalLanguagesCollection.Add(new OriginalLanguagesCollection
                                {
                                    VnMainModel =
                                        new VnMainModel { OriginalLanguages = new BitmapImage(new Uri(language)) }
                                })));
                        }
                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Description =
                            ConvertRichTextDocument.ConvertToFlowDocument(vnInfo.Description))));
                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        DownloadCoverImage(vnInfo.ImageLink, Convert.ToBoolean(vnInfo.ImageNsfw));
                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Name =
                            vnInfo.Title)));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Original =
                            vnInfo.Original)));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Released =
                            vnInfo.Released)));
                        //await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Platforms =
                        //    vnInfo.Platforms)));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Aliases =
                            vnInfo.Aliases)));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Length =
                            vnInfo.Length)));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Popularity =
                            Math.Round(Convert.ToDouble(vnInfo.Popularity), 2))));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Rating =
                            Convert.ToInt32(vnInfo.Rating))));
                        break;
                    }


                    #endregion

                    #region VnIcon
                    await Application.Current.Dispatcher.BeginInvoke(
                        new Action((() => VnMainModel.VnIcon = LoadIcon())));
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region VnAnime

                    foreach (var anime in db.Set<EF.Entity.VnInfo.VnInfoAnime>()
                        .Where(v => v.VnId == Globals.VnId))
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() => VnInfoAnimeCollection.Add(
                            new VnInfoAnime
                            {
                                Title = anime.TitleEng,
                                OriginalName = anime.TitleJpn,
                                Year = anime.Year,
                                AnimeType = anime.AnimeType,
                                AniDb = $"anidb.net/a{anime.AniDbId}",
                                Ann = $"animenewsnetwork.com/encyclopedia/anime.php?id={anime.AnnId}",
                                //TODO: AnimeNFo not added because of inconsistant url naming scheme
                            })));
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnTags

                    foreach (string tag in db.Set<VnInfoTags>().Where(v => v.VnId == Globals.VnId)
                        .Select(n => n.TagName))
                    {
                        await Application.Current.Dispatcher.BeginInvoke(
                            new Action(() => VnInfoTagCollection.Add(tag)));
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnLinks

                    foreach (VnInfoLinks links in db.Set<VnInfoLinks>().Where(v => v.VnId == Globals.VnId))
                    {
                        string wikipedia = String.Empty;
                        string encubed = String.Empty;
                        string renai = String.Empty;
                        if (!string.IsNullOrEmpty(links.Wikipedia))
                        {
                            wikipedia = $@"[url=https://en.wikipedia.org/wiki/{links.Wikipedia}]Wikipedia[/url]";
                        }
                        if (!string.IsNullOrEmpty(links.Encubed))
                        {
                            encubed = $@"[url=http://novelnews.net/tag/{links.Encubed}]Encubed[/url]";
                        }
                        if (!string.IsNullOrEmpty(links.Renai))
                        {
                            renai = $@"[url=https://renai.us/game/{links.Renai}]Renai[/url]";
                        }
                        List<string> combinedList = new List<string> { wikipedia, encubed, renai };

                        string combined = string.Join(", ", combinedList.Where(s => !string.IsNullOrEmpty(s)));
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.Links =
                            combined)));
                    }


                    #endregion

                    #region VnRelations

                    foreach (VnInfoRelations relation in db.Set<VnInfoRelations>().Where(v => v.VnId == Globals.VnId))
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action((() => this.VnInfoRelation.Add(
                            new VnInfoRelation
                            {
                                Title = relation.Title,
                                Original = relation.Original,
                                Relation = relation.Relation,
                                Official = relation.Official
                            }))));
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnUserData

                    foreach (var userData in db.Set<VnUserData>().Where(v => v.VnId == Globals.VnId))
                    {
                        if (string.IsNullOrEmpty(userData.LastPlayed))
                        {
                            await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.LastPlayed =
                                "Never")));
                        }
                        else
                        {
                            if ((Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days > -7
                            ) //need to set to negative, for the difference in days
                            {
                                if (Convert.ToDateTime(userData.LastPlayed) == DateTime.Today)
                                {
                                    await Application.Current.Dispatcher.BeginInvoke(
                                        new Action((() => VnMainModel.LastPlayed = "Today")));
                                }
                                else if ((Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days > -2 &&
                                         (Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days < 0)
                                {
                                    await Application.Current.Dispatcher.BeginInvoke(
                                        new Action((() => VnMainModel.LastPlayed = "Yesterday")));
                                }
                                else
                                {
                                    await Application.Current.Dispatcher.BeginInvoke(new Action(
                                        (() => VnMainModel.LastPlayed =
                                            Convert.ToDateTime(userData.LastPlayed).DayOfWeek.ToString())));
                                }
                            }
                            else
                            {
                                await Application.Current.Dispatcher.BeginInvoke(
                                    new Action((() => VnMainModel.LastPlayed = userData.LastPlayed)));
                            }
                        }



                        string[] splitPlayTime =
                            userData.PlayTime.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> timeList = new List<int>(4);
                        timeList.AddRange(splitPlayTime.Select(time => Convert.ToInt32(time)));
                        TimeSpan timeSpan = new TimeSpan(timeList[0], timeList[1], timeList[2], timeList[3]);

                        if (timeSpan < new TimeSpan(0, 0, 0, 1))
                        {
                            await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.PlayTime =
                                "Never")));
                        }
                        if (timeSpan < new TimeSpan(0, 0, 0, 60))
                        {
                            await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.PlayTime =
                                "Less than 1 minute")));
                        }
                        else
                        {
                            string formatted =
                                $"{(timeSpan.Duration().Days > 0 ? $"{timeSpan.Days:0} day{(timeSpan.Days == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                                $"{(timeSpan.Duration().Hours > 0 ? $"{timeSpan.Hours:0} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                                $"{(timeSpan.Duration().Minutes > 0 ? $"{timeSpan.Minutes:0} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")} " : string.Empty)}";
                            await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.PlayTime =
                                formatted)));
                        }
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion
                    db.Dispose();
                }
            }
            catch (Exception exception)
            {
                DebugLogging.WriteDebugLog(exception);
                throw;
            }
            finally
            {
                if (Globals.StatusBar.ProgressPercentage != null)
                    Globals.StatusBar.ProgressPercentage = 100;
                await Application.Current.Dispatcher.BeginInvoke(new Action((() => Globals.StatusBar.ProgressStatus =
                    new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\ok.png")))));
                Globals.StatusBar.ProgressText = "Done";
                await Task.Delay(1500);
                Globals.StatusBar.ProgressStatus = null;
                Globals.StatusBar.ProgressPercentage = null;
                Globals.StatusBar.IsDbProcessing = false;
                Globals.StatusBar.IsWorkProcessing = false;
                Globals.StatusBar.ProgressText = string.Empty;
            }
        }
    }

    //class for context menu
    public partial class VnMainViewModel
    {
        private void CreateContextMenu()
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(CreateAddSubMenu("Add To Category"));
            contextMenu.Items.Add(CreateRemoveSubMenu("Remove Category"));
            contextMenu.Items.Add(new MenuItem { Header = "Item with gesture", InputGestureText = "Ctrl+C" });
            contextMenu.Items.Add(new MenuItem { Header = "Item, disabled", IsEnabled = false });
            contextMenu.Items.Add(new MenuItem { Header = "Item, checked", IsChecked = true });
            contextMenu.Items.Add(new MenuItem { Header = "Item, checked and disabled", IsChecked = true, IsEnabled = false });
            contextMenu.Items.Add(new Separator());
            var menu = CreateAddSubMenu("Item with Submenu, disabled");
            contextMenu.Items.Add(menu);
            menu.IsEnabled = false;
            contextMenu.IsOpen = true;
        }

        private MenuItem CreateAddSubMenu(string header)
        {
            var item = new MenuItem { Header = header };

            using (var context = new DatabaseContext())
            {
                //get a list of all category names that are linked to the selected Vn
                List<string> data = context.VnUserCategoryTitles.Where(x => x.Title == SelectedVn)
                    .SelectMany(x => x.CategoryJunctions.Select(y=>y.Category.CategoryName)).ToList();
                foreach (var categories in context.Set<Category>())
                {
                    //prevents adding to All or the category currently loaded, or any categories already added
                    if (categories.CategoryName != "All" && categories.CategoryName != _selectedCategory && !data.Contains(categories.CategoryName))
                    {
                        item.Items.Add(new MenuItem { Header = categories.CategoryName, Command = AddToCategoryCommand, CommandParameter = categories.CategoryName });
                    }
                }
            }
            return item;
        }

        private MenuItem CreateRemoveSubMenu(string header)
        {
            var item = new MenuItem { Header = header };
            using (var context = new DatabaseContext())
            {
                List<Category> data = context.VnUserCategoryTitles.Where(x => x.Title == SelectedVn)
                    .SelectMany(x => x.CategoryJunctions.Select(y => y.Category)).ToList();
                foreach (var categories in data)
                {
                    if (categories.CategoryName != "All")
                    {
                        item.Items.Add(new MenuItem { Header = categories.CategoryName, Command = RemoveFromCategoryCommand, CommandParameter = categories.CategoryName });
                    }
                }

            }
            return item;
        }

        private void AddToCategory(string header)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        //create a relationship between the two members
                        var category = context.Categories.FirstOrDefault(x => x.CategoryName == header);
                        var vnUser = context.VnUserCategoryTitles.FirstOrDefault(v => v.Title == SelectedVn);
                        if (category != null && vnUser != null)
                        {
                            var addCategoryVnEntry = new CategoryJunction { Category = category, VnUserCategoryTitle = vnUser };
                            context.CategoryJunction.Add(addCategoryVnEntry);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }


        private void RemoveFromCategory(string header)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        CategoryJunction data = context.CategoryJunction
                            .FirstOrDefault(x => x.Category.CategoryName == header && x.VnUserCategoryTitle.Title == SelectedVn);
                        if (data != null)
                        {
                            context.CategoryJunction.Remove(data);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

    }

    //class for properties
    public partial class VnMainViewModel
    {
        #region ObservableVnNameCollection
        private RangeEnabledObservableCollection<string> _vnNameCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> VnNameCollection
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

        #region ObservablePlatformCollection
        private ObservableCollection<PlatformCollection> _platformCollection = new ObservableCollection<PlatformCollection>();
        public ObservableCollection<PlatformCollection> PlatformCollection
        {
            get { return _platformCollection; }
            set
            {
                _platformCollection = value;
                RaisePropertyChanged(nameof(PlatformCollection));
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

        #region ObservableCategories
        private RangeEnabledObservableCollection<string> _categoriesCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> CategoriesCollection
        {
            get
            {
                //TODO:move this to another method maybe?
                using (var db = new DatabaseContext())
                {
                    foreach (Category category in db.Set<Category>())
                    {
                        _categoriesCollection.Add(category.CategoryName);
                    }
                    db.Dispose();
                }
                return _categoriesCollection;
            }
            set
            {
                _categoriesCollection = value;
                RaisePropertyChanged(nameof(CategoriesCollection));
            }
        }

        #endregion ObservableCategories
        

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

        #region SelectedCategory
        private string _selectedCategory;
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                LoadCategories();
                RaisePropertyChanged(nameof(SelectedCategory));
            }
        }
        #endregion

        public static bool IsDownloading = false;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        public ICommand StartVnCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(StartVn);
        public ICommand OpenContextMenuCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(CreateContextMenu);
        public ICommand AddRemoveCategoryCommand => new GalaSoft.MvvmLight.CommandWpf.RelayCommand(LoadAddRemoveCategoryWindow);
        public ICommand AddToCategoryCommand { get; private set; }
        public ICommand RemoveFromCategoryCommand { get; private set; }

    }


    public class LanguagesCollection
    {
        public VnMainModel VnMainModel { get; set; }
    }

    public class OriginalLanguagesCollection
    {
        public VnMainModel VnMainModel { get; set; }
    }

    public class PlatformCollection
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
