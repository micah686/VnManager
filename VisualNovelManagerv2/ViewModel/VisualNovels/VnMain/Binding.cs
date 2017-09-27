using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMain
{
    //class for bindings
    public partial class VnMainViewModel
    {
        private void BindCoverImage(string url, bool? nsfw)
        {
            string pathNoExt = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}";
            string path = $@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg";

            try
            {
                if (!File.Exists(path) && !File.Exists(pathNoExt)) return;
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
                        
                        BitmapImage bitmap = new BitmapImage();
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }
                        VnMainModel.Image = bitmap;
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
                        TagDescription = ConvertTextBBcode.ConvertText(tag);
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
                    foreach (VnInfo vnInfo in db.Set<VnInfo>().Where(t => t.Title == (_selectedVn)))
                    {
                        Globals.VnId = vnInfo.VnId;


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
                            ConvertTextBBcode.ConvertText(vnInfo.Description))));
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
                    List<string> tagNames = (from info in db.VnInfoTags where info.VnId.Equals(Globals.VnId) join tag in db.VnTagData on info.TagId equals tag.TagId select tag.Name).ToList();
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() => VnInfoTagCollection.InsertRange(tagNames)));

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
                            await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.LastPlayed ="Never")));
                        }
                        else
                        {
                            if ((Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days > -7) //need to set to negative, for the difference in days
                            {
                                if (Convert.ToDateTime(userData.LastPlayed) == DateTime.Today)
                                {
                                    await Application.Current.Dispatcher.BeginInvoke(
                                        new Action((() => VnMainModel.LastPlayed = "Today")));
                                }
                                else if ((Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days > -2 &&(Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days < 0)
                                {
                                    await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.LastPlayed = "Yesterday")));
                                }
                                else
                                {
                                    await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.LastPlayed = Convert.ToDateTime(userData.LastPlayed).DayOfWeek.ToString())));
                                }
                            }
                            else
                            {
                                await Application.Current.Dispatcher.BeginInvoke(new Action((() => VnMainModel.LastPlayed = userData.LastPlayed)));
                            }
                        }



                        string[] splitPlayTime = userData.PlayTime.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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

        private IEnumerable<string> GetLangauges(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\Unknown.png")
                .ToList();
        }

        private IEnumerable<string> GetPlatforms(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(plat => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\platforms\{plat}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\platforms\{plat}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\platforms\Unknown.png")
                .ToList();
        }
    }
}
