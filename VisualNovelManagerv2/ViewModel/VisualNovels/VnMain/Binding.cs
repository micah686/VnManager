using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VisualNovelManagerv2.Model.VisualNovel;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMain
{
    //class for bindings
    public partial class VnMainViewModel
    {
        private void BindCoverImage(bool? nsfw)
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
                        BitmapImage bImage = Globals.NsfwEnabled ? Base64Converter.GetBitmapImageFromBytes(File.ReadAllText(pathNoExt))
                            : new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\nsfw\cover.jpg"));
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
                Globals.Logger.Error(ex);
                throw;
            }
        }

        private void BindTagDescription()
        {
            try
            {
                if (!(_selectedTagIndex >= 0)) return;
                using (var context = new DatabaseContext())
                {
                    foreach (string tag in context.VnTagData.Where(n => n.Name == SelectedTag).Select(d => d.Description))
                    {
                        TagDescription = ConvertTextBBcode.ConvertText(tag);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }
        }

        public void BindVnDataPublic()
        {
            BindVnData();
        }

        private void BindVnData()
        {
            IsMainBinding = true;
            
            try
            {
                Globals.StatusBar.ProgressText = "Loading Main Data";
                double ProgressIncrement = 11.11111111111111;
                Globals.StatusBar.ProgressPercentage = 0;
                Globals.StatusBar.IsWorkProcessing = true;
                using (var context = new DatabaseContext())
                {


                    #region VnInfo
                    foreach (VnInfo vnInfo in context.VnInfo.Where(t => t.Title == (_selectedVn)))
                    {
                        Globals.VnId = vnInfo.VnId;


                        foreach (string language in GetLangauges(vnInfo.Languages))
                        {
                            LanguageCollection.Add(new LanguagesCollection
                            {
                                VnMainModel = new VnMainModel {Languages = new BitmapImage(new Uri(language))}
                            });
                        }

                        foreach (string platform in GetPlatforms(vnInfo.Platforms))
                        {
                            PlatformCollection.Add(new PlatformCollection
                            {
                                VnMainModel = new VnMainModel {Platforms = new BitmapImage(new Uri(platform))}
                            });
                        }

                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        foreach (string language in GetLangauges(vnInfo.OriginalLanguage))
                        {
                            OriginalLanguagesCollection.Add(new OriginalLanguagesCollection
                            {
                                VnMainModel =
                                    new VnMainModel {OriginalLanguages = new BitmapImage(new Uri(language))}
                            });
                        }
                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        VnMainModel.Description = ConvertTextBBcode.ConvertText(vnInfo.Description);
                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        BindCoverImage(Convert.ToBoolean(vnInfo.ImageNsfw));
                        if (Globals.StatusBar.ProgressPercentage != null)
                            Globals.StatusBar.ProgressPercentage =
                                (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                        VnMainModel.Name = vnInfo.Title;
                        VnMainModel.Original = vnInfo.Original;
                        VnMainModel.Released = vnInfo.Released;
                        VnMainModel.Aliases = vnInfo.Aliases;
                        switch (vnInfo.Length)
                        {
                            case "VeryShort":
                                VnMainModel.Length = "Very short";
                                break;
                            case "VeryLong":
                                VnMainModel.Length = "Very long";
                                break;
                            default:
                                VnMainModel.Length = vnInfo.Length;
                                break;
                        }
                        VnMainModel.Popularity = Math.Round(Convert.ToDouble(vnInfo.Popularity), 2);
                        VnMainModel.Rating = Convert.ToInt32(vnInfo.Rating);
                        break;
                    }


                    #endregion

                    #region VnIcon
                    VnMainModel.VnIcon = LoadIcon();
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region VnAnime

                    foreach (var anime in context.VnInfoAnime.Where(v => v.VnId == Globals.VnId))
                    {
                        VnInfoAnimeCollection.Add(
                            new VnInfoAnime
                            {
                                Title = anime.TitleEng,
                                OriginalName = anime.TitleJpn,
                                Year = anime.Year,
                                AnimeType = anime.AnimeType,
                                AniDb = $"anidb.net/a{anime.AniDbId}",
                                Ann = $"animenewsnetwork.com/encyclopedia/anime.php?id={anime.AnnId}",
                                //TODO: AnimeNFo not added because of inconsistant url naming scheme
                            });
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnTags

                    string[] tagNames = (from info in context.VnInfoTags
                        where info.VnId.Equals(Globals.VnId)
                        where info.Spoiler <= Globals.MaxSpoiler
                        join tag in context.VnTagData on info.TagId equals tag.TagId
                        select tag.Name).Distinct().ToArray();
                    VnInfoTagCollection.InsertRange(tagNames);

                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnLinks

                    foreach (VnInfoLinks links in context.VnInfoLinks.Where(v => v.VnId == Globals.VnId))
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
                        VnMainModel.Links = combined;
                    }


                    #endregion

                    #region VnRelations

                    foreach (VnInfoRelations relation in context.VnInfoRelations.Where(v => v.VnId == Globals.VnId))
                    {
                        VnInfoRelation.Add(
                            new VnInfoRelation
                            {
                                Title = relation.Title,
                                Original = relation.Original,
                                Relation = relation.Relation,
                                Official = relation.Official
                            });
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnUserData

                    foreach (var userData in context.VnUserData.Where(v => v.VnId == Globals.VnId))
                    {
                        if (string.IsNullOrEmpty(userData.LastPlayed))
                        {
                            VnMainModel.LastPlayed = "Never";
                        }
                        else
                        {
                            if ((Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days > -7) //need to set to negative, for the difference in days
                            {
                                if (Convert.ToDateTime(userData.LastPlayed) == DateTime.Today)
                                {
                                    VnMainModel.LastPlayed = "Today";
                                }
                                else if ((Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days > -2 &&(Convert.ToDateTime(userData.LastPlayed) - DateTime.Today).Days < 0)
                                {
                                    VnMainModel.LastPlayed = "Yesterday";
                                }
                                else
                                {
                                    VnMainModel.LastPlayed = Convert.ToDateTime(userData.LastPlayed).DayOfWeek.ToString();
                                }
                            }
                            else
                            {
                                VnMainModel.LastPlayed = userData.LastPlayed;
                            }
                        }



                        string[] splitPlayTime = userData.PlayTime.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                            string formatted =
                                $"{(timeSpan.Duration().Days > 0 ? $"{timeSpan.Days:0} day{(timeSpan.Days == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                                $"{(timeSpan.Duration().Hours > 0 ? $"{timeSpan.Hours:0} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                                $"{(timeSpan.Duration().Minutes > 0 ? $"{timeSpan.Minutes:0} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")} " : string.Empty)}";
                            VnMainModel.PlayTime = formatted;
                        }
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion
                }
                if (Globals.StatusBar.ProgressPercentage != null)
                    Globals.StatusBar.ProgressPercentage = 100;
                Globals.StatusBar.ProgressText = "Done";
                Task.Delay(1500).Wait();
                Globals.StatusBar.ProgressPercentage = null;
                Globals.StatusBar.IsDbProcessing = false;
                Globals.StatusBar.IsWorkProcessing = false;
                Globals.StatusBar.ProgressText = string.Empty;
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
            finally
            {
                
                IsMainBinding = false;
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
