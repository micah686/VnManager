using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.CustomClasses.Vndb;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnCharacter;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnRelease;
using VisualNovelManagerv2.EF.Entity.VnTagTrait;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VisualNovelManagerv2.ViewModel.VisualNovels.VnMain;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Common;
using VndbSharp.Models.Dumps;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;
using static System.Globalization.CultureInfo;


namespace VisualNovelManagerv2.CustomClasses.Database
{
    public class AddToDatabase
    {
        private uint _vnid;
        private string _exepath;
        private string _iconpath;
        private double ProgressIncrement = 0;

        public async void GetId(uint id, string exe, string icon)
        {
            _vnid = id;
            _exepath = exe;
            _iconpath = icon;
            await GetData();
            
        }

        async Task GetData()
        {

            Globals.StatusBar.ProgressPercentage = 0;
            Globals.StatusBar.IsDbProcessing = true;
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.ProgressText = "Processing";
            using (VndbSharp.Vndb client = new VndbSharp.Vndb(true).WithClientDetails(Globals.ClientInfo[0], Globals.ClientInfo[1]))
            {
                try
                {
                    bool hasMore = true;
                    RequestOptions ro = new RequestOptions();
                    int count = 1;

                    #region GetIncrement

                    int characterCount = 0;
                    while (hasMore)
                    {
                        ro.Page = count;
                        VndbResponse<Character> characterList = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.Basic, ro);
                        hasMore = characterList.HasMore;
                        characterCount = characterCount + characterList.Count;
                        count++;
                    }
                    hasMore = true;
                    count = 1;
                    double statusCount = (10 + characterCount);

                    #endregion

                    ProgressIncrement = 100 / statusCount;

                    #region VisualNovels

                    VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(_vnid), VndbFlags.FullVisualNovel);
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region Releases

                    Collection<Release> releases = new BindingList<Release>();
                    while (hasMore)
                    {
                        ro.Page = count;
                        VndbResponse<Release> releaseList =
                            await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullRelease,
                                ro);
                        hasMore = releaseList.HasMore;
                        foreach (Release release in releaseList)
                        {
                            releases.Add(release);
                        }
                        count++;
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    hasMore = true;
                    count = 1;

                    #region Characters

                    Collection<Character> characters = new BindingList<Character>();
                    while (hasMore)
                    {
                        ro.Page = count;
                        var characterList = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid),
                            VndbFlags.FullCharacter, ro);
                        hasMore = characterList.HasMore;
                        foreach (Character character in characterList)
                        {
                            characters.Add(character);
                        }
                        count++;
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    await Task.Run((() => AddDataToDb(visualNovels, releases, characters)));
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    Globals.StatusBar.ProgressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\error.png"));
                    Globals.StatusBar.ProgressText = "An Error Occured! Check log for details";
                    throw;
                }
                finally
                {
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = 100;
                    Globals.StatusBar.ProgressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\ok.png"));
                    Globals.StatusBar.ProgressText = "Done";
                    await Task.Delay(1500);
                    Globals.StatusBar.ProgressStatus = null;
                    Globals.StatusBar.ProgressPercentage = null;
                    Globals.StatusBar.IsDbProcessing = false;
                    Globals.StatusBar.IsWorkProcessing = false;
                    Globals.StatusBar.ProgressText = string.Empty;
                    VnMainViewModel.ClearCollectionsCommand.Execute(null);
                    VnMainViewModel.LoadBindVnDataCommand.Execute(null);
                }
            }
        }



        async Task AddDataToDb(VndbResponse<VisualNovel> visualNovels, Collection<Release> releases, Collection<Character> characters)
        {
            using (var db = new DatabaseContext())
            {
                try
                {
                    #region VnInfo
                    var vninfo = db.Set<VnInfo>();
                    var vninfoanime = db.Set<EF.Entity.VnInfo.VnInfoAnime>();
                    var vninfolinks = db.Set<VnInfoLinks>();
                    var vninfotags = db.Set<VnInfoTags>();
                    var vntagdata = db.Set<VnTagData>();
                    var vnscreens = db.Set<VnInfoScreens>();
                    var vninforelations = db.Set<VnInfoRelations>();
                    var vninfostaff = db.Set<VnInfoStaff>();
                    foreach (VisualNovel visualNovel in visualNovels)
                    {
                        #region VnInfo
                        vninfo.Add(new VnInfo
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
                            vninfoanime.Add(new EF.Entity.VnInfo.VnInfoAnime
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
                        vninfolinks.Add(new VnInfoLinks
                        {
                            VnId = Convert.ToInt32(visualNovel.Id),
                            Wikipedia = visualNovel.VisualNovelLinks.Wikipedia,
                            Encubed = visualNovel.VisualNovelLinks.Encubed,
                            Renai = visualNovel.VisualNovelLinks.Renai
                        });
                        #endregion

                        #region VnTags
                        IEnumerable<Tag> tagMatches = null;
                        if (visualNovel.Tags.Count > 0)
                        {
                            tagMatches = await GetDetailsFromTagDump(visualNovel.Tags);
                            if (Globals.StatusBar.ProgressPercentage != null)
                                Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                            int count = 0;
                            foreach (TagMetadata tag in visualNovel.Tags)
                            {
                                //makes sure the matches contains tagid before proceeding, prevents crashing with tags waiting for approval
                                if (tagMatches.Any(c => c.Id == tag.Id))
                                {
                                    vninfotags.Add(new VnInfoTags
                                    {
                                        VnId = visualNovel.Id,
                                        TagId = Convert.ToInt32(tag.Id),
                                        TagName = tagMatches.ElementAt(count).Name,
                                        Score = tag.Score,
                                        Spoiler = tag.SpoilerLevel.ToString()
                                    });
                                    count++;
                                }
                            }
                        }


                        #endregion

                        #region VnTagData

                        if (tagMatches != null)
                        {
                            foreach (Tag tag in tagMatches)
                            {
                                vntagdata.Add(new VnTagData
                                {
                                    TagId = Convert.ToInt32(tag.Id),
                                    Name = tag.Name,
                                    Description = tag.Description,
                                    Meta = tag.IsMeta.ToString(),
                                    Vns = Convert.ToInt32(tag.VisualNovels),
                                    Cat = tag.TagCategory.ToString(),
                                    Aliases = ConvertToCsv(tag.Aliases),
                                    Parents = string.Join(",", tag.Parents)
                                });
                            }
                        }


                        #endregion

                        #region VnScreens
                        if (visualNovel.Screenshots.Count > 0)
                        {
                            foreach (ScreenshotMetadata screenshot in visualNovel.Screenshots)
                            {
                                vnscreens.Add(new VnInfoScreens
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
                                vninforelations.Add(new VnInfoRelations
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
                                vninfostaff.Add(new VnInfoStaff
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
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region VnCharacter
                    IEnumerable<Trait> traitDump = await VndbUtils.GetTraitsDumpAsync();
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    var vncharacter = db.Set<VnCharacter>();
                    var vncharactertraits = db.Set<VnCharacterTraits>();
                    var vntraitdata = db.Set<VnTraitData>();
                    var vncharactervns = db.Set<VnCharacterVns>();
                    foreach (Character character in characters)
                    {
                        #region VnCharacter
                        vncharacter.Add(new VnCharacter
                        {
                            VnId = _vnid,
                            CharacterId = Convert.ToInt32(character.Id),
                            Name = character.Name,
                            Original = character.OriginalName,
                            Gender = character.Gender.ToString(),
                            BloodType = character.BloodType.ToString(),
                            Birthday = ConvertBirthday(character.Birthday),
                            Aliases = ConvertToCsv(character.Aliases),
                            Description = character.Description,
                            ImageLink = character.Image,
                            Bust = Convert.ToInt32(character.Bust),
                            Waist = Convert.ToInt32(character.Waist),
                            Hip = Convert.ToInt32(character.Hip),
                            Height = Convert.ToInt32(character.Height),
                            Weight = Convert.ToInt32(character.Weight)
                        });
                        #endregion

                        #region VnCharacterTraits

                        if (character.Traits.Count <= 0) continue;
                        IEnumerable<Trait> traitMatches = GetDetailsFromTraitDump(traitDump, character.Traits);
                        int count = 0;
                        foreach (TraitMetadata trait in character.Traits)
                        {
                            if (traitMatches.Any(c => c.Id == trait.Id))//prevents crashes with awaiting traits
                            {
                                vncharactertraits.Add(new VnCharacterTraits
                                {
                                    CharacterId = Convert.ToInt32(character.Id),
                                    TraitId = Convert.ToInt32(trait.Id),
                                    TraitName = traitMatches.ElementAt(count).Name,
                                    SpoilerLevel = trait.SpoilerLevel.ToString() ?? null,
                                });
                                count++;
                            }
                        }

                        #endregion

                        #region TraitMatches
                        if (traitMatches != null)
                        {
                            foreach (Trait trait in traitMatches)
                            {
                                vntraitdata.Add(new VnTraitData
                                {
                                    TraitId = Convert.ToInt32(trait.Id),
                                    Name = trait.Name,
                                    Description = trait.Description,
                                    Meta = trait.IsMeta.ToString(),
                                    Chars = Convert.ToInt32(trait.Characters),
                                    Aliases = ConvertToCsv(trait.Aliases),
                                    Parents = string.Join(",", trait.Parents)
                                });
                            }
                        }


                        #endregion

                        #region VnCharacterVns
                        foreach (VndbSharp.Models.Character.VisualNovelMetadata vn in character.VisualNovels)
                        {
                            vncharactervns.Add(new VnCharacterVns
                            {
                                CharacterId = Convert.ToInt32(character.Id),
                                VnId = vn.Id,
                                ReleaseId = Convert.ToInt32(vn.ReleaseId),
                                SpoilerLevel = vn.SpoilerLevel.ToString() ?? null,
                                Role = vn.Role.ToString()
                            });
                        }
                        #endregion

                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region VnRelease
                    var vnrelease = db.Set<VnRelease>();
                    var vnreleasemedia = db.Set<VnReleaseMedia>();
                    var vnreleaseproducers = db.Set<VnReleaseProducers>();
                    var vnreleasevn = db.Set<VnReleaseVn>();

                    foreach (Release release in releases)
                    {
                        #region VnRelease
                        vnrelease.Add(new VnRelease
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
                        #endregion

                        #region VnReleaseMedia
                        if (release.Media.Count <= 0) continue;
                        foreach (Media media in release.Media)
                        {
                            vnreleasemedia.Add(new VnReleaseMedia
                            {
                                ReleaseId = Convert.ToInt32(release.Id),
                                Medium = media.Medium,
                                Quantity = Convert.ToInt32(media.Quantity)
                            });
                        }


                        #endregion

                        #region VnReleaseProducers
                        if (release.Producers.Count <= 0) continue;
                        foreach (ProducerRelease producer in release.Producers)
                        {
                            vnreleaseproducers.Add(new VnReleaseProducers
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
                        #endregion

                        #region VnReleaseVn
                        if (release.VisualNovels.Count <= 0) continue;
                        foreach (VndbSharp.Models.Release.VisualNovelMetadata vn in release.VisualNovels)
                        {
                            vnreleasevn.Add(new VnReleaseVn
                            {
                                ReleaseId = Convert.ToInt32(release.Id),
                                VnId = _vnid,
                                Name = vn.Name,
                                Original = vn.OriginalName
                            });
                        }
                        #endregion
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    #region VnUserData
                    var vnuserdata = db.Set<VnUserData>();
                    vnuserdata.Add(new VnUserData
                    {
                        VnId = _vnid,
                        ExePath = _exepath,
                        IconPath = _iconpath,
                        LastPlayed = "",
                        PlayTime = "0,0,0,0"
                    });

                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    throw;
                }
                db.Dispose();
            }
        }

        private async Task<IEnumerable<Tag>> GetDetailsFromTagDump(ReadOnlyCollection<TagMetadata> tags)
        {
            IEnumerable<Tag> tagDump = await VndbUtils.GetTagsDumpAsync();
            IEnumerable<Tag> matches = from tagMetadata in tags from tTag in tagDump where tTag.Id == tagMetadata.Id select tTag;
            //the above does these foreach loops, only a LOT faster
            //foreach (var tag in tags){ foreach (var tgTag in tagDump){ if (tgTag.Id == tag.Id){ } } };
            if (Globals.StatusBar.ProgressPercentage != null)
                Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;
            return matches;
        }

        private IEnumerable<Trait> GetDetailsFromTraitDump(IEnumerable<Trait> traitDump, ReadOnlyCollection<TraitMetadata> traits)
        {

            IEnumerable<Trait> matches = from traitMetadata in traits
                                         from tTrait in traitDump
                                         where tTrait.Id == traitMetadata.Id
                                         select tTrait;
            if (Globals.StatusBar.ProgressPercentage != null)
                Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;
            return matches;
        }

        private string ConvertBirthday(SimpleDate birthday)
        {
            string formatted = string.Empty;
            if (birthday == null) return formatted;
            if (birthday.Month == null) return birthday.Month == null ? birthday.Day.ToString() : string.Empty;
            string month = CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(birthday.Month));
            formatted = $"{month} {birthday.Day}";
            return formatted;
        }

        string ConvertToCsv(ReadOnlyCollection<string> input)
        {
            return input != null ? string.Join(",", input) : null;
        }
    }
}
