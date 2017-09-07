using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnRelease;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Common;
using VndbSharp.Models.Dumps;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;
using System.Globalization;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.AddVn
{
    public partial class AddVnViewModel
    {

        private async Task AddToDatabase(VndbResponse<VisualNovel> visualNovels, List<Release> releases, List<Character> characters)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    foreach (VisualNovel visualNovel in visualNovels)
                    {
                        #region VnInfo
                        context.VnInfo.Add(new VnInfo
                        {
                            VnId = visualNovel.Id,
                            Title = visualNovel.Name,
                            Original = visualNovel.OriginalName,
                            Released = visualNovel.Released?.ToString() ?? null,
                            Languages = ConvertToCsv(visualNovel.Languages),
                            OriginalLanguage = ConvertToCsv(visualNovel.OriginalLanguages),
                            Platforms = ConvertToCsv(visualNovel.Platforms),
                            Aliases = ConvertToCsv(visualNovel.Aliases),
                            Length = visualNovel.Length?.ToString(),
                            Description = visualNovel.Description,
                            ImageLink = visualNovel.Image,
                            ImageNsfw = visualNovel.IsImageNsfw.ToString(),
                            Popularity = visualNovel.Popularity,
                            Rating = Convert.ToInt32(visualNovel.Rating)
                        });
                        #endregion

                        #region VnInfoAnime
                        foreach (AnimeMetadata anime in visualNovel.Anime)
                        {
                            context.VnInfoAnime.Add(new EF.Entity.VnInfo.VnInfoAnime
                            {
                                VnId = Convert.ToInt32(visualNovel.Id),
                                AniDbId = anime.AniDbId,
                                AnnId = anime.AnimeNewsNetworkId,
                                AniNfoId = anime.AnimeNfoId,
                                TitleEng = anime.RomajiTitle,
                                TitleJpn = anime.KanjiTitle,
                                Year = anime.AiringYear?.ToString() ?? null,
                                AnimeType = anime.Type
                            });
                        }
                        #endregion

                        #region VnInfoLinks
                        context.VnInfoLinks.Add(new VnInfoLinks
                        {
                            VnId = Convert.ToInt32(visualNovel.Id),
                            Wikipedia = visualNovel.VisualNovelLinks.Wikipedia,
                            Encubed = visualNovel.VisualNovelLinks.Encubed,
                            Renai = visualNovel.VisualNovelLinks.Renai
                        });
                        #endregion

                        #region VnTags
                        if (visualNovel.Tags.Count > 0)
                        {

                            // this query SHOULD be implementing this: foreach (TagMetadata tag in visualNovel.Tags){if (tagMatches.Any(c => c.Id == tag.Id)){} }
                            //checks if the dump was downloaded, or if 24 hours have passed
                            if (_didDownloadTagDump.Item1 == false || Math.Abs(_didDownloadTagDump.Item2.Subtract(DateTime.Now).TotalHours) <= 24)
                            {
                                List<Tag> tagDump = (await VndbUtils.GetTagsDumpAsync()).ToList();
                                //TODO: check for duplicates
                                foreach (Tag tag in tagDump)
                                {
                                    context.VnTagData.Add(new VnTagData
                                    {
                                        TagId = tag.Id,
                                        Name = tag.Name,
                                        Description = tag.Description,
                                        Meta = tag.IsMeta.ToString(),
                                        Vns = tag.VisualNovels,
                                        Cat = tag.TagCategory.ToString(),
                                        Aliases = ConvertToCsv(tag.Aliases),
                                        Parents = tag.Parents != null ? string.Join(",", tag.Parents) : null,
                                    });
                                }
                                //gets a list of all tags from the TagDump where the dump contains the tagIds from the vn
                                List<TagMetadata> vnInfoTags = (from tag in visualNovel.Tags from ef in tagDump where tag.Id == ef.Id select tag).ToList();
                                foreach (TagMetadata tagMetadata in vnInfoTags)
                                {
                                    context.VnInfoTags.Add(new VnInfoTags
                                    {
                                        VnId = _vnid,
                                        TagId = tagMetadata.Id,
                                        Score = tagMetadata.Score,
                                        Spoiler = tagMetadata.SpoilerLevel.ToString()
                                    });
                                }

                                //gets a list of items from VnTagData where it contains the tagId from the vn
                                List<VnTagData> matches = (from ef in context.VnTagData from tag in visualNovel.Tags where ef.TagId == tag.Id select ef).ToList();
                            }
                            else
                            {
                                //gets a list of all tags from the VnTagData where that data contains the tagId from the vn
                                List<TagMetadata> vnInfoTags = (from tag in visualNovel.Tags from ef in context.VnTagData where tag.Id == ef.TagId select tag).ToList();
                                foreach (TagMetadata tagMetadata in vnInfoTags)
                                {
                                    context.VnInfoTags.Add(new VnInfoTags
                                    {
                                        VnId = _vnid,
                                        TagId = tagMetadata.Id,
                                        Score = tagMetadata.Score,
                                        Spoiler = tagMetadata.SpoilerLevel.ToString()
                                    });
                                }

                                //gets a list of items from VnTagData where it contains the tagId from the vn
                                List<VnTagData> matches = (from ef in context.VnTagData from tag in visualNovel.Tags where ef.TagId == tag.Id select ef).ToList();
                            }
                        }


                        #endregion


                        #region VnScreens
                        if (visualNovel.Screenshots.Count > 0)
                        {
                            foreach (ScreenshotMetadata screenshot in visualNovel.Screenshots)
                            {
                                context.VnInfoScreens.Add(new VnInfoScreens
                                {
                                    VnId = visualNovel.Id,
                                    ImageUrl = screenshot.Url,
                                    ReleaseId = screenshot.ReleaseId,
                                    Nsfw = screenshot.IsNsfw.ToString(),
                                    Height = screenshot.Height,
                                    Width = screenshot.Width
                                });
                            }
                        }
                        #endregion

                        #region VnInfoRelations
                        if (visualNovel.Relations.Count > 0)
                        {
                            foreach (VisualNovelRelation relation in visualNovel.Relations)
                            {
                                context.VnInfoRelations.Add(new VnInfoRelations
                                {
                                    VnId = Convert.ToInt32(visualNovel.Id),
                                    RelationId = relation.Id,
                                    Relation = relation.Type.ToString(),
                                    Title = relation.Title,
                                    Original = relation.Original,
                                    Official = relation.Official ? "Yes" : "No"
                                });
                            }
                        }
                        #endregion

                        #region VnInfoStaff
                        if (visualNovel.Staff.Count > 0)
                        {
                            foreach (StaffMetadata staff in visualNovel.Staff)
                            {
                                context.VnInfoStaff.Add(new VnInfoStaff
                                {
                                    VnId = visualNovel.Id,
                                    StaffId = Convert.ToInt32(staff.StaffId),
                                    AliasId = Convert.ToInt32(staff.AliasId),
                                    Name = staff.Name,
                                    Original = staff.Kanji,
                                    Role = staff.Role,
                                    Note = staff.Note
                                });
                            }
                        }
                        #endregion
                    }


                    #region VnRelease

                    foreach (Release release in releases)
                    {
                        context.VnRelease.Add(new EF.Entity.VnRelease.VnRelease
                        {
                            VnId = _vnid,
                            ReleaseId = Convert.ToInt32(release.Id),
                            Title = release.Name,
                            Original = release.OriginalName,
                            Released = release.Released.ToString(),
                            ReleaseType = release.Type.ToString(),
                            Patch = release.IsPatch.ToString(),
                            Freeware = release.IsFreeware.ToString(),
                            Doujin = release.IsDoujin.ToString(),
                            Languages = ConvertToCsv(release.Languages),
                            Website = release.Website,
                            Notes = release.Notes,
                            MinAge = Convert.ToInt32(release.MinimumAge),
                            Gtin = release.Gtin,
                            Catalog = release.Catalog,
                            Platforms = ConvertToCsv(release.Platforms),
                            Resolution = release.Resolution,
                            Voiced = release.Voiced.ToString(),
                            Animation = string.Join(",", release.Animation)
                        });

                        if (release.Media.Count <= 0) continue;
                        foreach (Media media in release.Media)
                        {
                            context.VnReleaseMedia.Add(new VnReleaseMedia
                            {
                                ReleaseId = Convert.ToInt32(release.Id),
                                Medium = media.Medium,
                                Quantity = Convert.ToInt32(media.Quantity)
                            });
                        }

                        if (release.Producers.Count <= 0) continue;
                        foreach (ProducerRelease producer in release.Producers)
                        {
                            context.VnReleaseProducers.Add(new VnReleaseProducers
                            {
                                ReleaseId = Convert.ToInt32(release.Id),
                                ProducerId = Convert.ToInt32(producer.Id),
                                Developer = producer.IsDeveloper.ToString(),
                                Publisher = producer.IsPublisher.ToString(),
                                Name = producer.Name,
                                Original = producer.OriginalName,
                                ProducerType = producer.ProducerType
                            });
                        }

                        if (release.VisualNovels.Count <= 0) continue;
                        foreach (VndbSharp.Models.Release.VisualNovelMetadata vn in release.VisualNovels)
                        {
                            context.VnReleaseVn.Add(new VnReleaseVn
                            {
                                ReleaseId = Convert.ToInt32(release.Id),
                                VnId = _vnid,
                                Name = vn.Name,
                                Original = vn.OriginalName
                            });
                        }
                    }
                    

                    #endregion End VnRelease

                    #region UserData
                    context.VnUserData.Add(new VnUserData
                    {
                        VnId = _vnid,
                        ExePath = FileName,
                        IconPath = IconName,
                        LastPlayed = String.Empty,
                        PlayTime = "0,0,0,0"
                    });
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }




        private async Task<List<VnTagData>> GetDetailsFromTagDump(ReadOnlyCollection<TagMetadata> tags)
        {
            using (var context = new DatabaseContext())
            {
                //checks if the dump was downloaded, or if 24 hours have passed
                if (_didDownloadTagDump.Item1 == false || Math.Abs(_didDownloadTagDump.Item2.Subtract(DateTime.Now).TotalHours) <= 24)
                {
                    List<Tag> tagDump = (await VndbUtils.GetTagsDumpAsync()).ToList();
                    foreach (Tag tag in tagDump)
                    {
                        context.VnTagData.Add(new VnTagData
                        {
                            TagId = tag.Id,
                            Name = tag.Name,
                            Description = tag.Description,
                            Meta = tag.IsMeta.ToString(),
                            Vns = tag.VisualNovels,
                            Cat = tag.TagCategory.ToString(),
                            Aliases = ConvertToCsv(tag.Aliases),
                            Parents = tag.Parents != null ? string.Join(",", tag.Parents) : null,
                        });
                    }                    
                    //gets a list of all tags from the TagDump where the dump contains the tagIds from the vn
                    List<TagMetadata> vnInfoTags = (from tag in tags from ef in tagDump where tag.Id == ef.Id select tag).ToList();
                    foreach (TagMetadata tagMetadata in vnInfoTags)
                    {
                        context.VnInfoTags.Add(new VnInfoTags
                        {
                            VnId = _vnid,
                            TagId = tagMetadata.Id,
                            Score = tagMetadata.Score,
                            Spoiler = tagMetadata.SpoilerLevel.ToString()
                        });
                    }

                    //gets a list of items from VnTagData where it contains the tagId from the vn
                    List<VnTagData> matches = (from ef in context.VnTagData from tag in tags where ef.TagId == tag.Id select ef).ToList();
                    return matches;
                }
                else
                {
                    //gets a list of all tags from the VnTagData where that data contains the tagId from the vn
                    List<TagMetadata> vnInfoTags = (from tag in tags from ef in context.VnTagData where tag.Id == ef.TagId select tag).ToList();
                    foreach (TagMetadata tagMetadata in vnInfoTags)
                    {
                        context.VnInfoTags.Add(new VnInfoTags
                        {
                            VnId = _vnid,
                            TagId = tagMetadata.Id,
                            Score = tagMetadata.Score,
                            Spoiler = tagMetadata.SpoilerLevel.ToString()
                        });
                    }

                    //gets a list of items from VnTagData where it contains the tagId from the vn
                    List<VnTagData> matches = (from ef in context.VnTagData from tag in tags where ef.TagId == tag.Id select ef).ToList();
                    return matches;
                }                
            }
            
        }

        private IEnumerable<Trait> GetDetailsFromTraitDump(IEnumerable<Trait> traitDump, ReadOnlyCollection<TraitMetadata> traits)
        {

            IEnumerable<Trait> matches = from traitMetadata in traits
                from tTrait in traitDump
                where tTrait.Id == traitMetadata.Id
                select tTrait;
            if (Globals.StatusBar.ProgressPercentage != null)
                Globals.StatusBar.ProgressPercentage =
                    (double) Globals.StatusBar.ProgressPercentage + _progressIncrement;
            return matches;
        }

        private string ConvertBirthday(SimpleDate birthday)
        {
            string formatted = string.Empty;
            if (birthday == null) return formatted;
            if (birthday.Month == null) return birthday.Month == null ? birthday.Day.ToString() : string.Empty;
            string month = System.Globalization.DateTimeFormatInfo.InvariantInfo.GetMonthName(Convert.ToInt32(birthday.Month));
            formatted = $"{month} {birthday.Day}";
            return formatted;
        }

        string ConvertToCsv(ReadOnlyCollection<string> input)
        {
            return input != null ? string.Join(",", input) : null;
        }
    }
}
