using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using VndbSharp;
using VndbSharp.Models.Character;
using VndbSharp.Models.Dumps;
using VndbSharp.Models.Producer;
using VndbSharp.Models.Release;
using VndbSharp.Models.Staff;
using VndbSharp.Models.VisualNovel;
using VnManager.Converters;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db.Vndb.Producer;
using VnManager.Models.Db.Vndb.Release;
using VnManager.Models.Db.Vndb.Staff;
using VnManager.Models.Db.Vndb.TagTrait;

namespace VnManager.MetadataProviders.Vndb
{
    public class SaveVnDataToDb
    {

        public void SortVnInfo()
        {
            //sort out info like collection type, ift's already been added,...
        }


        #region VnInfo
        public void SaveVnInfo(VisualNovel visualNovel)
        {
            if (visualNovel == null) return;
            try
            {
                using (var db = new LiteDatabase(App.DatabasePath))
                {
                    var dbVnInfo = db.GetCollection<VnInfo>("vninfo");
                    var dbVnInfoAnime = db.GetCollection<VnInfoAnime>("vninfo_anime");
                    var dbVnInfoLinks = db.GetCollection<VnInfoLinks>("vninfo_links");
                    var dbVnInfoScreens = db.GetCollection<VnInfoScreens>("vninfo_screens");
                    var dbVnInfoRelations = db.GetCollection<VnInfoRelations>("vninfo_relations");
                    var dbVnInfoStaff = db.GetCollection<VnInfoStaff>("vninfo_staff");
                    var dbVnInfoTags = db.GetCollection<VnInfoTags>("vninfo_tags");

                    var prevVnInfo = dbVnInfo.Query().Where(x => x.VnId == visualNovel.Id).FirstOrDefault();
                    List<VnInfoAnime> prevVnInfoAnime = dbVnInfoAnime.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    var prevVnInfoLinks = dbVnInfoLinks.Query().Where(x => x.VnId == visualNovel.Id).FirstOrDefault();
                    List<VnInfoScreens> prevVnInfoScreens = dbVnInfoScreens.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    List<VnInfoRelations> prevVnInfoRelations = dbVnInfoRelations.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    List<VnInfoStaff> prevVnInfoStaff = dbVnInfoStaff.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    List<VnInfoTags> prevVnInfoTags = dbVnInfoTags.Query().Where(x => x.VnId == visualNovel.Id).ToList();

                    List<VnInfoTags> vnTags = new List<VnInfoTags>();
                    List<VnInfoStaff> vnStaff = new List<VnInfoStaff>();
                    List<VnInfoRelations> vnRelations = new List<VnInfoRelations>();
                    List<VnInfoScreens> vnScreenshot = new List<VnInfoScreens>();
                    List<VnInfoAnime> vnAnime = new List<VnInfoAnime>();

                    var vn = new VnInfo();
                    if (prevVnInfo != null)
                    {
                        vn = prevVnInfo;
                    }

                    vn.VnId = visualNovel.Id;
                    vn.Title = visualNovel.Name;
                    vn.Original = visualNovel.OriginalName;
                    vn.Released = visualNovel.Released?.ToString() ?? null;
                    vn.Languages = CsvConverter.ConvertToCsv(visualNovel.Languages);
                    vn.OriginalLanguages = CsvConverter.ConvertToCsv(visualNovel.OriginalLanguages);
                    vn.Platforms = CsvConverter.ConvertToCsv(visualNovel.Platforms);
                    vn.Aliases = CsvConverter.ConvertToCsv(visualNovel.Aliases);
                    vn.Length = visualNovel.Length?.ToString();
                    vn.Description = visualNovel.Description;
                    vn.ImageLink = visualNovel.Image;
                    vn.ImageNsfw = visualNovel.IsImageNsfw;
                    vn.Popularity = visualNovel.Popularity;
                    vn.Rating = visualNovel.Rating;



                    //anime
                    vnAnime.AddRange(FormatVnInfoAnime(visualNovel, prevVnInfoAnime));


                    //links
                    VnInfoLinks vnLinks = new VnInfoLinks();
                    if (prevVnInfoLinks != null)
                    {
                        vnLinks = prevVnInfoLinks;
                    }

                    vnLinks.VnId = visualNovel.Id;
                    vnLinks.Wikidata = visualNovel.VisualNovelLinks.Wikidata;
                    vnLinks.Encubed = visualNovel.VisualNovelLinks.Encubed;
                    vnLinks.Renai = visualNovel.VisualNovelLinks.Renai;


                    //screenshot
                    if (visualNovel.Screenshots.Count > 0)
                    {
                        vnScreenshot.AddRange(FormatVnInfoScreens(visualNovel, prevVnInfoScreens));
                    }

                    //relations
                    if (visualNovel.Relations.Count > 0)
                    {
                        vnRelations.AddRange(FormatVnInfoRelations(visualNovel, prevVnInfoRelations));
                    }

                    //staff
                    if (visualNovel.Staff.Count > 0)
                    {
                        vnStaff.AddRange(FormatVnInfoStaff(visualNovel, prevVnInfoStaff));
                    }


                    if (visualNovel.Tags.Count > 0)
                    {
                        vnTags.AddRange(FormatVnInfoTags(visualNovel, prevVnInfoTags));
                    }

                    dbVnInfo.Upsert(vn);
                    dbVnInfoAnime.Upsert(vnAnime);
                    dbVnInfoLinks.Upsert(vnLinks);
                    dbVnInfoScreens.Upsert(vnScreenshot);
                    dbVnInfoRelations.Upsert(vnRelations);
                    dbVnInfoStaff.Upsert(vnStaff);
                    dbVnInfoTags.Upsert(vnTags);
                }
            }
            catch (IOException e)
            {
                App.Logger.Error(e, "an I/O exception occured");
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Something happened");
            }
        }

        private List<VnInfoAnime> FormatVnInfoAnime(VisualNovel visualNovel, List<VnInfoAnime> prevVnInfoAnime)
        {
            List<VnInfoAnime> vnAnime = new List<VnInfoAnime>();
            foreach (var anime in visualNovel.Anime)
            {
                var entry = prevVnInfoAnime.FirstOrDefault(x => x.AniDbId == anime.AniDbId) ??
                            new VnInfoAnime();

                entry.VnId = visualNovel.Id;
                entry.AniDbId = anime.AniDbId;
                entry.AnnId = anime.AnimeNewsNetworkId;
                entry.AniNfoId = anime.AnimeNfoId;
                entry.TitleEng = anime.RomajiTitle;
                entry.TitleJpn = anime.KanjiTitle;
                entry.AnimeType = anime.Type;
                vnAnime.Add(entry);
            }

            return vnAnime;
        }

        private List<VnInfoScreens> FormatVnInfoScreens(VisualNovel visualNovel, List<VnInfoScreens> prevVnInfoScreens)
        {
            List<VnInfoScreens> vnScreenshot = new List<VnInfoScreens>();
            foreach (var screenshot in visualNovel.Screenshots)
            {
                var entry = prevVnInfoScreens.FirstOrDefault(x => x.ImageUrl == screenshot.Url) ??
                            new VnInfoScreens();
                entry.VnId = visualNovel.Id;
                entry.ImageUrl = screenshot.Url;
                entry.ReleaseId = screenshot.ReleaseId;
                entry.Nsfw = screenshot.IsNsfw;
                entry.Height = screenshot.Height;
                entry.Width = screenshot.Width;
                vnScreenshot.Add(entry);
            }

            return vnScreenshot;
        }

        private List<VnInfoRelations> FormatVnInfoRelations(VisualNovel visualNovel, List<VnInfoRelations> prevVnInfoRelations)
        {
            List<VnInfoRelations> vnRelations = new List<VnInfoRelations>();
            foreach (var relation in visualNovel.Relations)
            {
                var entry = prevVnInfoRelations.FirstOrDefault(x => x.RelationId == relation.Id) ??
                            new VnInfoRelations();
                entry.VnId = visualNovel.Id;
                entry.RelationId = relation.Id;
                entry.Relation = relation.Type.ToString();
                entry.Title = relation.Title;
                entry.Original = relation.Original;
                entry.Official = relation.Official ? "Yes" : "No";
                vnRelations.Add(entry);
            }

            return vnRelations;
        }

        private List<VnInfoStaff> FormatVnInfoStaff(VisualNovel visualNovel, List<VnInfoStaff> prevVnInfoStaff)
        {
            List<VnInfoStaff> vnStaff = new List<VnInfoStaff>();
            foreach (var staff in visualNovel.Staff)
            {
                var entry = prevVnInfoStaff.FirstOrDefault(x => x.StaffId == staff.StaffId) ??
                            new VnInfoStaff();
                entry.VnId = visualNovel.Id;
                entry.StaffId = staff.StaffId;
                entry.AliasId = staff.AliasId;
                entry.Name = staff.Name;
                entry.Original = staff.Kanji;
                entry.Role = staff.Role;
                entry.Note = staff.Note;
                vnStaff.Add(entry);
            }
            return vnStaff;
        }

        private List<VnInfoTags> FormatVnInfoTags(VisualNovel visualNovel, List<VnInfoTags> prevVnInfoTags)
        {
            List<VnInfoTags> vnTags = new List<VnInfoTags>();
            foreach (var tag in visualNovel.Tags)
            {
                var entry = prevVnInfoTags.FirstOrDefault(x => x.TagId == tag.Id) ?? new VnInfoTags();
                entry.VnId = visualNovel.Id;
                entry.TagId = tag.Id;
                entry.Score = tag.Score;
                entry.Spoiler = tag.SpoilerLevel;
                vnTags.Add(entry);
            }

            return vnTags;
        }
        #endregion

        #region VnCharacters
        public void SaveVnCharacters(List<Character> characters, uint vnid)
        {
            if (characters.Count < 1) return;
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbCharInfo = db.GetCollection<VnCharacterInfo>("vncharacter");
                var dbCharTraits = db.GetCollection<VnCharacterTraits>("vncharacter_traits");
                var dbCharVns = db.GetCollection<VnCharacterVns>("vncharacter_vns");
                var dbCharVoices = db.GetCollection<VnCharacterVoiced>("vncharacter_voiced");
                var dbCharInstances = db.GetCollection<VnCharacterInstances>("vncharacter_instances");

                if (characters.Count > 0)
                {
                    List<VnCharacterInfo> vnCharactersList = new List<VnCharacterInfo>();
                    List<VnCharacterTraits> vnCharacterTraitsList = new List<VnCharacterTraits>();
                    List<VnCharacterVns> vnCharacterVnsList = new List<VnCharacterVns>();
                    List<VnCharacterVoiced> vnCharacterVoicesList = new List<VnCharacterVoiced>();
                    List<VnCharacterInstances> vnCharacterInstancesList = new List<VnCharacterInstances>();
                    foreach (Character vnCharacter in characters)
                    {
                        var prevVnCharacter = dbCharInfo.Query().Where(x => x.CharacterId == vnCharacter.Id);
                        var character = prevVnCharacter.FirstOrDefault() ?? new VnCharacterInfo();

                        character.VnId = vnid;
                        character.CharacterId = vnCharacter.Id;
                        character.Name = vnCharacter.Name;
                        character.Original = vnCharacter.OriginalName;
                        character.Gender = vnCharacter.Gender.ToString();
                        character.BloodType = vnCharacter.BloodType.ToString();
                        character.Birthday = BirthdayConverter.ConvertBirthday(vnCharacter.Birthday);
                        character.Aliases = CsvConverter.ConvertToCsv(vnCharacter.Aliases);
                        character.Description = vnCharacter.Description;
                        character.ImageLink = vnCharacter.Image;
                        character.Bust = Convert.ToInt32(vnCharacter.Bust);
                        character.Waist = Convert.ToInt32(vnCharacter.Waist);
                        character.Hip = Convert.ToInt32(vnCharacter.Hip);
                        character.Height = Convert.ToInt32(vnCharacter.Height);
                        character.Weight = Convert.ToInt32(vnCharacter.Weight);
                        vnCharactersList.Add(character);


                        if (vnCharacter.Traits.Count > 0)
                        {
                            ILiteQueryable<VnCharacterTraits> prevVnCharacterTraits =
                                dbCharTraits.Query().Where(x => x.CharacterId == vnCharacter.Id);
                            vnCharacterTraitsList.AddRange(FormatVnCharacterTraits(vnCharacter, prevVnCharacterTraits));
                        }


                        if (vnCharacter.VisualNovels.Count > 0)
                        {
                            ILiteQueryable<VnCharacterVns> prevVnCharacterVns = dbCharVns.Query().Where(x => x.CharacterId == vnCharacter.Id);
                            vnCharacterVnsList.AddRange(FormatVnCharacterVns(vnCharacter, prevVnCharacterVns));
                        }

                        if (vnCharacter.VoiceActorMetadata.Count > 0)
                        {
                            ILiteQueryable<VnCharacterVoiced> prevVnCharacterVoices =
                                dbCharVoices.Query().Where(x => x.CharacterId == vnCharacter.Id);
                            vnCharacterVoicesList.AddRange(FormatVnCharacterVoiced(vnCharacter, prevVnCharacterVoices));
                        }

                        if (vnCharacter.CharacterInstances.Count > 0)
                        {
                            ILiteQueryable<VnCharacterInstances> prevVnCharacterInstances =
                                dbCharInstances.Query().Where(x => x.CharacterId == vnCharacter.Id);
                            vnCharacterInstancesList.AddRange(FormatVnCharacterInstances(vnCharacter, prevVnCharacterInstances));
                        }


                    }

                    dbCharInfo.Upsert(vnCharactersList);
                    dbCharTraits.Upsert(vnCharacterTraitsList);
                    dbCharVns.Upsert(vnCharacterVnsList);
                    dbCharVoices.Upsert(vnCharacterVoicesList);
                    dbCharInstances.Upsert(vnCharacterInstancesList);
                }
            }

        }

        private List<VnCharacterTraits> FormatVnCharacterTraits(Character vnCharacter, ILiteQueryable<VnCharacterTraits> prevVnCharacterTraits)
        {
            List<VnCharacterTraits> vnCharacterTraitsList = new List<VnCharacterTraits>();
            var entry = prevVnCharacterTraits.FirstOrDefault() ?? new VnCharacterTraits();
            foreach (var traits in vnCharacter.Traits)
            {
                entry.CharacterId = vnCharacter.Id;
                entry.TraitId = traits.Id;
                entry.SpoilerLevel = traits.SpoilerLevel;
                vnCharacterTraitsList.Add(entry);
            }
            return vnCharacterTraitsList;
        }

        private List<VnCharacterVns> FormatVnCharacterVns(Character vnCharacter, ILiteQueryable<VnCharacterVns> prevVnCharacterVns)
        {
            List<VnCharacterVns> vnCharacterVnsList = new List<VnCharacterVns>();
            var entry = prevVnCharacterVns.FirstOrDefault() ?? new VnCharacterVns();
            foreach (var vn in vnCharacter.VisualNovels)
            {
                entry.CharacterId = vnCharacter.Id;
                entry.VnId = vn.Id;
                entry.ReleaseId = vn.ReleaseId;
                entry.SpoilerLevel = vn.SpoilerLevel;
                entry.Role = vn.Role.ToString();
                vnCharacterVnsList.Add(entry);
            }
            return vnCharacterVnsList;
        }

        private List<VnCharacterVoiced> FormatVnCharacterVoiced(Character vnCharacter, ILiteQueryable<VnCharacterVoiced> prevVnCharacterVoices)
        {
            List<VnCharacterVoiced> vnCharacterVoicesList = new List<VnCharacterVoiced>();
            var entry = prevVnCharacterVoices.FirstOrDefault() ?? new VnCharacterVoiced();

            foreach (var voice in vnCharacter.VoiceActorMetadata)
            {
                entry.CharacterId = (int)vnCharacter.Id;
                entry.StaffId = voice.StaffId;
                entry.StaffAliasId = voice.AliasId;
                entry.VnId = voice.VisualNovelId;
                entry.Note = voice.Note;
                vnCharacterVoicesList.Add(entry);
            }

            return vnCharacterVoicesList;
        }
        private List<VnCharacterInstances> FormatVnCharacterInstances(Character vnCharacter, ILiteQueryable<VnCharacterInstances> prevVnCharacterInstances)
        {
            List<VnCharacterInstances> vnCharacterInstancesList = new List<VnCharacterInstances>();
            var entry = prevVnCharacterInstances.FirstOrDefault() ?? new VnCharacterInstances();

            foreach (var instance in vnCharacter.CharacterInstances)
            {
                entry.CharacterId = (int)vnCharacter.Id;
                entry.Name = instance.Name;
                entry.Original = instance.Kanji;
                entry.Spoiler = instance.Spoiler;
                vnCharacterInstancesList.Add(entry);
            }

            return vnCharacterInstancesList;
        }
        #endregion

        #region VnReleases
        public void SaveVnReleases(List<Release> vnReleases)
        {
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbVnRelease = db.GetCollection<VnRelease>("vnrelease");
                var dbVnReleaseMedia = db.GetCollection<VnReleaseMedia>("vnrelease_media");
                var dbVnReleaseProducers = db.GetCollection<VnReleaseProducers>("vnrelease_producers");
                var dbReleaseVns = db.GetCollection<VnReleaseVn>("vnrelease_vn");
                if (vnReleases.Count <= 0) return;
                List<VnRelease> vnReleaseList = new List<VnRelease>();
                List<VnReleaseMedia> vnReleaseMediaList = new List<VnReleaseMedia>();
                List<VnReleaseProducers> vnReleaseProducersList = new List<VnReleaseProducers>();
                List<VnReleaseVn> vnReleaseVnsList = new List<VnReleaseVn>();
                foreach (Release vnRelease in vnReleases)
                {
                    var prevVnRelease = dbVnRelease.Query().Where(x => x.ReleaseId == vnRelease.Id);
                    var release = prevVnRelease.FirstOrDefault() ?? new VnRelease();

                    release.ReleaseId = vnRelease.Id;
                    release.Title = vnRelease.Name;
                    release.Original = vnRelease.OriginalName;
                    release.ReleaseType = vnRelease.Type.ToString();
                    release.Patch = vnRelease.IsPatch;
                    release.Freeware = vnRelease.IsFreeware;
                    release.Doujin = vnRelease.IsDoujin;
                    release.Languages = CsvConverter.ConvertToCsv(vnRelease.Languages);
                    release.Website = vnRelease.Website;
                    release.Notes = vnRelease.Notes;
                    release.MinAge = Convert.ToByte(vnRelease.MinimumAge);
                    release.Gtin = vnRelease.Gtin;
                    release.Catalog = vnRelease.Catalog;
                    release.Platforms = CsvConverter.ConvertToCsv(vnRelease.Platforms);
                    release.Resolution = vnRelease.Resolution;
                    release.Voiced = vnRelease.Voiced.ToString();
                    release.Animation = string.Join(",", vnRelease.Animation);
                    vnReleaseList.Add(release);


                    if (vnRelease.Media.Count > 0)
                    {
                        ILiteQueryable<VnReleaseMedia> prevVnReleaseMedia = dbVnReleaseMedia.Query().Where(x => x.ReleaseId == vnRelease.Id);
                        vnReleaseMediaList.AddRange(FormatVnProducersMedia(vnRelease, prevVnReleaseMedia));
                    }

                    if (vnRelease.Producers.Count > 0)
                    {
                        ILiteQueryable<VnReleaseProducers> prevVnReleaseProducers =
                            dbVnReleaseProducers.Query().Where(x => x.ReleaseId == vnRelease.Id);
                        vnReleaseProducersList.AddRange(FormatVnProducers(vnRelease, prevVnReleaseProducers));

                    }

                    if (vnRelease.VisualNovels.Count > 0)
                    {
                        ILiteQueryable<VnReleaseVn> prevVnReleaseVns = dbReleaseVns.Query().Where(x => x.ReleaseId == vnRelease.Id);
                        vnReleaseVnsList.AddRange(FormatVnReleaseVns(vnRelease, prevVnReleaseVns));
                    }

                }

                dbVnRelease.Upsert(vnReleaseList);
                dbVnReleaseMedia.Upsert(vnReleaseMediaList);
                dbVnReleaseProducers.Upsert(vnReleaseProducersList);
                dbReleaseVns.Upsert(vnReleaseVnsList);

            }

        }

        private List<VnReleaseMedia> FormatVnProducersMedia(Release vnRelease, ILiteQueryable<VnReleaseMedia> prevVnReleaseMedia)
        {
            List<VnReleaseMedia> vnReleaseMediaList = new List<VnReleaseMedia>();
            foreach (var media in vnRelease.Media)
            {
                var entry = prevVnReleaseMedia.FirstOrDefault() ?? new VnReleaseMedia();
                entry.ReleaseId = vnRelease.Id;
                entry.Medium = media.Medium;
                entry.Quantity = media.Quantity;
                vnReleaseMediaList.Add(entry);
            }
            return vnReleaseMediaList;
        }

        private List<VnReleaseProducers> FormatVnProducers(Release vnRelease, ILiteQueryable<VnReleaseProducers> prevVnReleaseProducers)
        {
            List<VnReleaseProducers> vnReleaseProducersList = new List<VnReleaseProducers>();
            foreach (var producer in vnRelease.Producers)
            {
                var entry = prevVnReleaseProducers.FirstOrDefault() ?? new VnReleaseProducers();
                entry.ReleaseId = vnRelease.Id;
                entry.ProducerId = producer.Id;
                entry.Developer = producer.IsDeveloper;
                entry.Publisher = producer.IsPublisher;
                entry.Name = producer.Name;
                entry.Original = producer.OriginalName;
                entry.ProducerType = producer.ProducerType;
                vnReleaseProducersList.Add(entry);
            }
            return vnReleaseProducersList;
        }

        private List<VnReleaseVn> FormatVnReleaseVns(Release vnRelease, ILiteQueryable<VnReleaseVn> prevVnReleaseVns)
        {
            List<VnReleaseVn> vnReleaseVnsList = new List<VnReleaseVn>();
            foreach (var vn in vnRelease.VisualNovels)
            {
                var entry = prevVnReleaseVns.FirstOrDefault() ?? new VnReleaseVn();
                entry.ReleaseId = vnRelease.Id;
                entry.VnId = vn.Id;
                entry.Name = vn.Name;
                entry.Original = vn.OriginalName;
                vnReleaseVnsList.Add(entry);

            }
            return vnReleaseVnsList;
        }
        #endregion

        #region VnProducers
        public void SaveProducers(List<Producer> vnProducers)
        {
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbVnProducer = db.GetCollection<VnProducer>("vnproducer");
                var dbVnProducerLinks = db.GetCollection<VnProducerLinks>("vnproducer_links");
                var dbVnProducerRelations = db.GetCollection<VnProducerRelations>("vnproducer_relations");

                List<VnProducer> vnProducersList = new List<VnProducer>();
                List<VnProducerLinks> vnProducerLinksList = new List<VnProducerLinks>();
                List<VnProducerRelations> vnProducerRelationsList = new List<VnProducerRelations>();
                foreach (var vnProducer in vnProducers)
                {
                    var prevVnProducers = dbVnProducer.Query().Where(x => x.ProducerId == vnProducer.Id);
                    var producer = prevVnProducers.FirstOrDefault() ?? new VnProducer();

                    producer.ProducerId = (int?)vnProducer.Id;
                    producer.Name = vnProducer.Name;
                    producer.Original = vnProducer.OriginalName;
                    producer.ProducerType = vnProducer.ProducerType;
                    producer.Language = vnProducer.Language;
                    producer.Aliases = CsvConverter.ConvertToCsv(vnProducer.Aliases);
                    producer.Description = vnProducer.Description;
                    vnProducersList.Add(producer);


                    var prevVnProducerLinks = dbVnProducerLinks.Query().Where(x => x.ProducerId == vnProducer.Id);
                    var links = prevVnProducerLinks.FirstOrDefault() ?? new VnProducerLinks();
                    links.ProducerId = (int)vnProducer.Id;
                    links.Homepage = vnProducer.Links.Homepage;
                    links.WikiData = vnProducer.Links.Wikipedia;
                    vnProducerLinksList.Add(links);

                    ILiteQueryable<VnProducerRelations> prevVnProducerRelations =
                        dbVnProducerRelations.Query().Where(x => x.ProducerId == vnProducer.Id);
                    vnProducerRelationsList.AddRange(FormatVnProducerRelations(vnProducer, prevVnProducerRelations));

                }

                dbVnProducer.Upsert(vnProducersList);
                dbVnProducerLinks.Upsert(vnProducerLinksList);
                dbVnProducerRelations.Upsert(vnProducerRelationsList);
            }
        }

        private List<VnProducerRelations> FormatVnProducerRelations(Producer vnProducer, ILiteQueryable<VnProducerRelations> prevVnProducerRelations)
        {
            List<VnProducerRelations> vnProducerRelationsList = new List<VnProducerRelations>();
            foreach (var relation in vnProducer.Relations)
            {
                var entry = prevVnProducerRelations.FirstOrDefault() ?? new VnProducerRelations();
                entry.RelationId = (int?)relation.Id;
                entry.ProducerId = (int?)vnProducer.Id;
                entry.Relation = relation.Relation;
                entry.Name = relation.Name;
                entry.Original = relation.OriginalName;
                vnProducerRelationsList.Add(entry);
            }
            return vnProducerRelationsList;
        }
        #endregion

        #region VnStaff
        public void SaveStaff(List<Staff> vnStaffList, int vnid)
        {
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbVnStaff = db.GetCollection<VnStaff>("vnstaff");
                var dbVnStaffAliases = db.GetCollection<VnStaffAliases>("vnstaff_aliases");
                var dbVnStaffVns = db.GetCollection<VnStaffVns>("vnstaff_vns");
                var dbVnStaffVoiced = db.GetCollection<VnStaffVoiced>("vnstaff_voiced");

                List<VnStaff> staffList = new List<VnStaff>();
                List<VnStaffAliases> vnStaffAliasesList = new List<VnStaffAliases>();
                List<VnStaffVns> vnStaffVnList = new List<VnStaffVns>();
                List<VnStaffVoiced> vnStaffVoicedList = new List<VnStaffVoiced>();

                foreach (var vnStaff in vnStaffList)
                {
                    //staff
                    var prevVnStaff = dbVnStaff.Query().Where(x => x.StaffId == vnStaff.Id);
                    var staff = prevVnStaff.FirstOrDefault() ?? new VnStaff();

                    staff.StaffId = (int?)vnStaff.Id;
                    staff.Name = vnStaff.Name;
                    staff.Original = vnStaff.OriginalName;
                    staff.Language = vnStaff.Language;
                    staff.VnStaffLinks = new VnStaffLinks()
                    {
                        Homepage = vnStaff.StaffLinks.Homepage,
                        Twitter = vnStaff.StaffLinks.Twitter,
                        AniDb = vnStaff.StaffLinks.AniDb,
                        Pixiv = vnStaff.StaffLinks.Pixiv,
                        Wikidata = vnStaff.StaffLinks.WikiData
                    };
                    staff.Description = vnStaff.Description;
                    staff.MainAliasId = vnStaff.MainAlias;
                    staffList.Add(staff);

                    //aliases
                    if (vnStaff.Aliases.Count > 0)
                    {
                        ILiteQueryable<VnStaffAliases> prevVnStaffAliases = dbVnStaffAliases.Query().Where(x => x.StaffId == vnStaff.Id);
                        vnStaffAliasesList.AddRange(FormatVnStaffAliases(vnStaff, prevVnStaffAliases));
                    }

                    //vns
                    if (vnStaff.Vns.Length > 0)
                    {
                        ILiteQueryable<VnStaffVns> prevVnStaffVns = dbVnStaffVns.Query().Where(x => x.StaffId == staff.StaffId);
                        vnStaffVnList.AddRange(FormatStaffVns(vnStaff, vnid, prevVnStaffVns));
                    }
                    //voiced
                    if (vnStaff.Voiced.Length > 0)
                    {
                        ILiteQueryable<VnStaffVoiced> prevVnStaffVoiced = dbVnStaffVoiced.Query().Where(x => x.StaffId == vnStaff.Id);
                        vnStaffVoicedList.AddRange(FormatVnStaffVoiced(vnStaff, vnid, prevVnStaffVoiced));
                    }

                }

                dbVnStaff.Upsert(staffList);
                dbVnStaffAliases.Upsert(vnStaffAliasesList);
                dbVnStaffVns.Upsert(vnStaffVnList);
                dbVnStaffVoiced.Upsert(vnStaffVoicedList);
            }
        }

        private List<VnStaffAliases> FormatVnStaffAliases(Staff vnStaff, ILiteQueryable<VnStaffAliases> prevVnStaffAliases)
        {
            List<VnStaffAliases> vnStaffAliasesList = new List<VnStaffAliases>();
            var vnAliases = prevVnStaffAliases.FirstOrDefault() ?? new VnStaffAliases();
            foreach (var alias in vnStaff.Aliases)
            {
                vnAliases.StaffId = (int?)vnStaff.Id;
                vnAliases.AliasId = (int)alias.Id;
                vnAliases.Name = alias.Name;
                vnAliases.Original = alias.OriginalName;
                vnStaffAliasesList.Add(vnAliases);
            }
            return vnStaffAliasesList;
        }
        private List<VnStaffVns> FormatStaffVns(Staff vnStaff, int vnid, ILiteQueryable<VnStaffVns> prevVnStaffVns)
        {
            List<VnStaffVns> vnStaffVnList = new List<VnStaffVns>();
            var staffVns = prevVnStaffVns.FirstOrDefault() ?? new VnStaffVns();
            foreach (var vn in vnStaff.Vns)
            {
                staffVns.VnId = vnid;
                staffVns.StaffId = (int?)vnStaff.Id;
                staffVns.AliasId = (int)staffVns.AliasId;
                staffVns.Role = vn.Role;
                staffVns.Note = vn.Note;
                vnStaffVnList.Add(staffVns);
            }
            return vnStaffVnList;
        }
        private List<VnStaffVoiced> FormatVnStaffVoiced(Staff vnStaff, int vnid, ILiteQueryable<VnStaffVoiced> prevVnStaffVoiced)
        {
            List<VnStaffVoiced> vnStaffVoicedList = new List<VnStaffVoiced>();
            foreach (var voiced in vnStaff.Voiced)
            {
                var entry = prevVnStaffVoiced.FirstOrDefault() ?? new VnStaffVoiced();
                entry.VnId = vnid;
                entry.StaffId = (int?)vnStaff.Id;
                entry.CharacterId = (int)voiced.CharacterId;
                entry.Note = voiced.Note;
                vnStaffVoicedList.Add(entry);
            }
            return vnStaffVoicedList;
        }
        #endregion








        //Tag and Trait Dumps
        public async Task GetAndSaveTagDump()
        {
            try
            {
                using (var db = new LiteDatabase(App.DatabasePath))
                {
                    var dbTags = db.GetCollection<VnTagData>("vntagdump");
                    List<Tag> tagDump = (await VndbUtils.GetTagsDumpAsync()).ToList();
                    List<VnTagData> tagsToAdd = new List<VnTagData>();
                    var prevEntry = dbTags.Query().ToList();

                    foreach (var item in tagDump)
                    {
                        var entry = prevEntry.FirstOrDefault(x => x.TagId == item.Id) ?? new VnTagData();
                        entry.TagId = item.Id;
                        entry.Name = item.Name;
                        entry.Description = item.Description;
                        entry.IsMeta = item.IsMeta;
                        entry.IsSearchable = item.Searchable;
                        entry.IsApplicable = item.Applicable;
                        entry.Vns = item.VisualNovels;
                        entry.Category = item.TagCategory;
                        entry.Aliases = CsvConverter.ConvertToCsv(item.Aliases);
                        entry.Parents = item.Parents.ToArray();
                        tagsToAdd.Add(entry);
                    }

                    dbTags.Upsert(tagsToAdd);
                    //remove any deleted tags
                    IEnumerable<int> idsToDelete = prevEntry.Except(tagsToAdd).Select(x => x.Index);
                    dbTags.DeleteMany(x => idsToDelete.Contains(x.Index));
                }

            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "An error happened while getting/saving the tag dump");
                throw;
            }
        }

        public async Task GetAndSaveTraitDump()
        {
            try
            {
                using (var db = new LiteDatabase(App.DatabasePath))
                {
                    var dbTraits = db.GetCollection<VnTraitData>("vntraitdump");
                    List<Trait> traitDump = (await VndbUtils.GetTraitsDumpAsync()).ToList();
                    List<VnTraitData> traitsToAdd = new List<VnTraitData>();
                    var prevEntry = dbTraits.Query().ToList();
                    foreach (var item in traitDump)
                    {
                        var entry = prevEntry.FirstOrDefault(x => x.TraitId == item.Id) ?? new VnTraitData();
                        entry.TraitId = item.Id;
                        entry.Name = item.Name;
                        entry.Description = item.Description;
                        entry.IsMeta = item.IsMeta;
                        //entry.IsSearchable = item.IsSearchable;
                        //entry.IsApplicable = item.IsApplicable;
                        entry.Characters = item.Characters;
                        entry.Aliases = CsvConverter.ConvertToCsv(item.Aliases);
                        entry.Parents = item.Parents.ToArray();
                        traitsToAdd.Add(entry);
                    }

                    dbTraits.Upsert(traitsToAdd);

                    IEnumerable<int> idsToDelete = prevEntry.Except(traitsToAdd).Select(x => x.Index);
                    dbTraits.DeleteMany(x => idsToDelete.Contains(x.Index));
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "An error happened while getting/saving the trait dump");
                throw;
            }
        }
    }
}
