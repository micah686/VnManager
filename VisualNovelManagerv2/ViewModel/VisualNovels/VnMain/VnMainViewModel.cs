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
using Microsoft.Practices.ServiceLocation;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnCharacter;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnRelease;
using Image = System.Drawing.Image;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;


// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMain
{
    public partial class VnMainViewModel : ViewModelBase
    {
        public static ICommand LoadBindVnDataCommand { get; private set; }
        public static ICommand ClearCollectionsCommand { get; private set; }
        public VnMainViewModel()
        {
            LoadBindVnDataCommand = new RelayCommand(LoadCategories);
            ClearCollectionsCommand = new RelayCommand(ClearVnData);
            AddToCategoryCommand = new RelayCommand<string>(AddToCategory);
            RemoveFromCategoryCommand = new RelayCommand<string>(RemoveFromCategory);
            //_vnMainModel = new VnMainModel();
            LoadCategories();
            
        }

        

        private void ClearVnData()
        {
            VnNameCollection.Clear();
            LanguageCollection.Clear();
            OriginalLanguagesCollection.Clear();
            VnInfoRelation.Clear();
            VnInfoTagCollection.Clear();
            VnInfoAnimeCollection.Clear();
            PlatformCollection.Clear();
            VnMainModel.Name = String.Empty;
            VnMainModel.Original = String.Empty;
            VnMainModel.PlayTime = String.Empty;
            VnMainModel.LastPlayed = String.Empty;
            VnMainModel.Image = null;
            VnMainModel.Aliases = String.Empty;
            VnMainModel.Description = new FlowDocument();
            VnMainModel.Released = String.Empty;
            VnMainModel.Length = String.Empty;
            VnMainModel.VnIcon = null;
            VnMainModel.Popularity = 0;
            VnMainModel.Rating = 0;
            VnMainModel.Links = string.Empty;

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
                        foreach (uint vnid in db.Set<VnInfo>().Where(t=>t.Title==(SelectedVn)).Select(v=>v.VnId))
                        {
                            Globals.VnId = vnid;
                        }
                        db.Dispose();
                    }

                    if (Globals.VnId > 0)
                    {
                        await Task.Run((BindVnData));
                        UpdateViews();
                    }
                    
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    throw;
                }
            }
            
        }

        

        private async void UpdateViews()
        {
            var cvm = ServiceLocator.Current.GetInstance<VnCharacterViewModel>();
            await Task.Run((() => cvm.DownloadCharactersCommand.Execute(null)));
            cvm.LoadCharacterCommand.Execute(null);
            
            var rvm = ServiceLocator.Current.GetInstance<VnReleaseViewModel>();
            rvm.LoadReleaseNamesCommand.Execute(null);

            var ssvm = ServiceLocator.Current.GetInstance<VnScreenshotViewModel>();
            await Task.Run((() => ssvm.DownloadScreenshotsCommand.Execute(null)));
            ssvm.BindScreenshotsCommand.Execute(null);
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
                switch (nsfw)
                {
                    case true:
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
                        break;
                    case false:
                        if (!File.Exists(path))
                        {
                            Globals.StatusBar.IsDownloading = true;
                            Thread.Sleep(1500);
                            WebClient client = new WebClient();
                            client.DownloadFile(new Uri(url), path);
                        }
                        break;
                }
                Globals.StatusBar.IsDownloading = false;
                await Application.Current.Dispatcher.BeginInvoke(new Action((() => BindCoverImage(url, nsfw))));
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
                    IsPlayEnabled = false;
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
                    vnUserData = context.VnUserData.FirstOrDefault(x => x.VnId.Equals(Globals.VnId));
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
            IsPlayEnabled = true;
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
