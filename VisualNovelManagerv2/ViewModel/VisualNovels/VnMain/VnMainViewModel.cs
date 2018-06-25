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
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VisualNovelManagerv2.Model.VisualNovel;
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
            LoadCategories();
            
        }

        

        private void ClearVnData()
        {
            TreeVnCategories.Clear();
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
            VnMainModel.Description = String.Empty;
            VnMainModel.Released = String.Empty;
            VnMainModel.Length = String.Empty;
            VnMainModel.VnIcon = null;
            VnMainModel.Popularity = 0;
            VnMainModel.Rating = 0;
            VnMainModel.Links = string.Empty;

        }

        public void LoadCategoriesPublic()
        {
            LoadCategories();
        }



        private void LoadCategories()
        {
            TreeVnCategories.Clear();

            try
            {
                using (var context = new DatabaseContext())
                {
                    if (context.VnInfo != null)
                    {
                        VnNameCollection.InsertRange(context.VnInfo.Select(x => x.Title).ToList());
                        return;
                    }

                }


                //using (var context = new DatabaseContext())
                //{
                //    MenuItem root = new MenuItem(){Header = "Visual Novels"};
                //    MenuItem all= new MenuItem(){Header = "All", IsSubmenuOpen = true};
                //    foreach (var item in context.VnInfo.Select(x => x.Title))
                //    {
                //        all.Items.Add(new MenuItem(){Header = item});
                //    }
                //    root.Items.Add(all);

                //    foreach (var category in context.Categories.Where(x => x.CategoryName != "All").Select(x => x.CategoryName))
                //    {
                //        var menuItem = new MenuItem(){Header = category};

                //        string[] names = context.VnInfo.Where(v => context.VnUserCategoryTitles.Where(c => c.Title == category).Select(x => x.VnId)
                //                .Contains(v.VnId)).Select(t => t.Title).ToArray();

                //        foreach (var vn in names)
                //        {
                //            menuItem.Items.Add(new MenuItem() {Header = vn});
                //        }
                //        root.Items.Add(menuItem);
                //    }
                //    TreeVnCategories.Add(root);

                //}
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
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
        



        private void CheckMenuItemName(object obj)
        {
            try
            {
                MenuItem menuItem = (MenuItem)obj;
                if (obj == null) return;
                if (obj.GetType() == typeof(MenuItem))
                {
                    string name = menuItem.Header.ToString();
                    _selectedVn = name;
                    if (menuItem.Parent != null)
                    {
                        if (menuItem.Parent.GetType() == typeof(MenuItem))
                        {
                            DatabaseContext context = new DatabaseContext();
                            if (!context.Categories.Select(x => x.CategoryName).Contains(name) && name != "Visual Novels")
                            {
                                GetVnData();
                            }
                            else
                            {
                                _selectedVn = String.Empty;
                            }
                            context.Dispose();
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

        private void GetVnData()
        {
            try
            {
                Globals.StatusBar.ProgressText = "Processing";
                LanguageCollection.Clear();
                PlatformCollection.Clear();
                OriginalLanguagesCollection.Clear();
                VnInfoRelation.Clear();
                VnInfoTagCollection.Clear();
                VnInfoAnimeCollection.Clear();
                TagDescription = String.Empty;

                using (var context = new DatabaseContext())
                {
                    Globals.VnId = context.VnInfo.Where(t => t.Title == (_selectedVn)).Select(v => v.VnId).FirstOrDefault();
                }

                if (Globals.VnId > 0)
                {

                    UpdateViews();
                }

            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                Globals.StatusBar.ProgressText= String.Empty;
                throw;
            }
        }

        

        private async void UpdateViews()
        {
            //only load when user input is enabled
            if (_isUserInputEnabled)
            {
                _isUserInputEnabled = false;
                IsPlayEnabled = false;
                Globals.StatusBar.ProgressText = "Loading Data";
                Globals.StatusBar.IsWorkProcessing = true;

                var cvm = ServiceLocator.Current.GetInstance<VnCharacterViewModel>();
                var ssvm = ServiceLocator.Current.GetInstance<VnScreenshotViewModel>();
                var rvm = ServiceLocator.Current.GetInstance<VnReleaseViewModel>();

                await Task.Run((DownloadCoverImage));
                BindVnData();
                await Task.Run((() => cvm.DownloadCharacterImagesPublic()));
                await Task.Run((() => ssvm.DonwloadScreenshotImagesPublic()));

                while (IsMainBinding && cvm.IsCharacterDownloading && ssvm.IsScreenshotDownloading)
                {
                    await Task.Delay(100);
                }

                cvm.ClearCharacterDataCommand.Execute(null);
                cvm.LoadCharacterCommand.Execute(null);

                rvm.ClearReleaseDataCommand.Execute(null);
                rvm.LoadReleaseNamesCommand.Execute(null);

                ssvm.BindScreenshotsCommand.Execute(null);
                Globals.StatusBar.ProgressText = String.Empty;
                //Globals.StatusBar.IsWorkProcessing = false;
                _isUserInputEnabled = true;
                IsPlayEnabled = true;
            }
            
        }

        private void SetMaxWidth()
        {
            if (VnNameCollection.Count > 0)
            {
                string longestString = VnNameCollection.OrderByDescending(s => s.Length).First();
                MaxListWidth = MeasureStringSize.GetMaxStringWidth(longestString);
            }

            if (VnInfoTagCollection.Count > 0)
            {
                string longestString = VnInfoTagCollection.OrderByDescending(s => s.Length).First();
                MinWidthTags = MeasureStringSize.GetMaxStringWidth(longestString);
            }
        }



        private BitmapSource LoadIcon()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    VnUserData userData = context.VnUserData.FirstOrDefault(v => v.VnId == Globals.VnId);
                    if (userData != null && Directory.Exists(userData.ExePath))
                    {
                        //checks for existance of Iconpath first, then ExePath. If both are null/empty, it returns a null image
                        return !string.IsNullOrEmpty(userData.IconPath) ? CreateIcon(userData.IconPath) : CreateIcon(!string.IsNullOrEmpty(userData.ExePath) ? userData.ExePath : null);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
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
                Globals.Logger.Error(ex);
                throw;
            }
            
        }

        private async Task DownloadCoverImage()
        {
            if (ConnectionTest.VndbTcpSocketTest() == false)
            {
                //TODO: add a default cover image for when it can't connect online
                Globals.Logger.Warn("Could not connect to Vndb API over SSL");
                Globals.StatusBar.SetOnlineStatusColor(false);
                Globals.StatusBar.IsShowOnlineStatusEnabled = true;
                await Task.Delay(3500);
                Globals.StatusBar.IsShowOnlineStatusEnabled = false;
                return;
            }
            using (var context = new DatabaseContext())
            {
                Globals.StatusBar.IsWorkProcessing = true;
                Globals.StatusBar.ProgressText = "Loading cover image";
                VnInfo vnData = context.VnInfo.FirstOrDefault(t => t.Title == (_selectedVn));
                if (vnData == null) return;
                string url = vnData.ImageLink;
                bool nsfw = Convert.ToBoolean(vnData.ImageNsfw);

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
                                Globals.StatusBar.ProgressText = "Loading cover image";
                                Thread.Sleep(150);//to be nice to the server
                                WebClient client = new WebClient();
                                using (MemoryStream stream = new MemoryStream(await client.DownloadDataTaskAsync(new Uri(url))))
                                {
                                    string base64Img = Base64Converter.ImageToBase64(Image.FromStream(stream), ImageFormat.Jpeg);
                                    File.WriteAllText(pathNoExt, base64Img);
                                }
                                client.Dispose();
                            }
                            break;
                        case false:
                            if (!File.Exists(path))
                            {
                                Globals.StatusBar.IsDownloading = true;
                                Globals.StatusBar.ProgressText = "Loading cover image";
                                Thread.Sleep(150);//to be nice to the server
                                WebClient client = new WebClient();
                                await client.DownloadFileTaskAsync(new Uri(url), path);
                                client.Dispose();
                            }
                            break;
                    }
                    Globals.StatusBar.IsDownloading = false;
                    Globals.StatusBar.ProgressText = String.Empty;
                }
                catch (WebException ex)
                {
                    Globals.Logger.Error(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    Globals.Logger.Error(ex);
                    throw;
                }
                Globals.StatusBar.IsWorkProcessing = false;
                Globals.StatusBar.ProgressText = String.Empty;
            }                        
        }

        private void StartVn()
        {
            try
            {
                if(Globals.VnId <1)
                    return;
                if (_isGameRunning)
                {
                    Messenger.Default.Send(new NotificationMessage("Game Already Running"));
                }
                if (_isGameRunning == false)
                {
                    _stopwatch.Reset();
                    Process process = null;
                    using (var context = new DatabaseContext())
                    {
                        VnInfo idList = context.VnInfo.FirstOrDefault(x => x.Title.Equals(_selectedVn));
                        VnUserData vnUserData = context.VnUserData.FirstOrDefault(x => x.VnId.Equals(idList.VnId));
                        if (vnUserData?.ExePath != null && Directory.Exists(vnUserData?.ExePath))
                        {
                            string exepath = vnUserData.ExePath;
                            string dirpath = Path.GetDirectoryName(exepath);
                            if (dirpath != null) Directory.SetCurrentDirectory(dirpath);
                            process = new Process
                            {
                                StartInfo =
                                {
                                    FileName = exepath, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
                                },
                                EnableRaisingEvents = true
                                
                            };
                            _processList.Add(process);
                            process.Start();
                            _isGameRunning = true;
                            _stopwatch.Start();
                            IsPlayEnabled = false;
                        }
                    }
                    Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
                    List<Process> children = process.GetChildProcesses();
                    _processList.AddRange(children);
                    int initialChildrenCount = children.Count;
                    if (process != null)
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += delegate (object o, EventArgs args)
                        {
                            //Only allows the HasExited event to trigger when there are no child processes
                            if (children.Count < 1 && initialChildrenCount == 0)
                            {
                                _stopwatch.Stop();
                                VnOrChildProcessExited(null, null);
                            }
                        };
                    }

                    foreach (Process proc in children)
                    {
                        proc.EnableRaisingEvents = true;
                        proc.Exited += VnOrChildProcessExited;
                    }
                }
                
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }            
        }

        private void VnOrChildProcessExited(object sender, EventArgs eventArgs)
        {
            //checks if the parent and all children have exited
            bool haveAllProcessesExited = _processList.All(x => x.HasExited);
            if (haveAllProcessesExited)
            {
                try
                {
                    _stopwatch.Stop();
                    _isGameRunning = false;
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
                    Globals.Logger.Error(ex);
                    throw;
                }
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
