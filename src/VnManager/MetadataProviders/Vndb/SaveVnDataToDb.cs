using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LiteDB;
using VndbSharp.Models.Character;
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

namespace VnManager.MetadataProviders.Vndb
{
    public class SaveVnDataToDb
    {

        public void SortVnInfo()
        {
            //sort out info like collection type, ift's already been added,...
        }


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
                    var prevVnInfoAnime = dbVnInfoAnime.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    var prevVnInfoLinks = dbVnInfoLinks.Query().Where(x => x.VnId == visualNovel.Id).FirstOrDefault();
                    var prevVnInfoScreens = dbVnInfoScreens.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    var prevVnInfoRelations = dbVnInfoRelations.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    var prevVnInfoStaff = dbVnInfoStaff.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    var prevVnInfoTags = dbVnInfoTags.Query().Where(x => x.VnId == visualNovel.Id).ToList();

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

                    dbVnInfo.Upsert(vn);

                    //anime
                    var vnAnime = new List<VnInfoAnime>();
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

                    dbVnInfoAnime.Upsert(vnAnime);


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
                    dbVnInfoLinks.Upsert(vnLinks);

                    //screenshot
                    if (visualNovel.Screenshots.Count > 0)
                    {
                        var vnScreenshot = new List<VnInfoScreens>();
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
                        dbVnInfoScreens.Upsert(vnScreenshot);
                    }

                    //relations
                    if (visualNovel.Relations.Count > 0)
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
                        dbVnInfoRelations.Upsert(vnRelations);
                    }

                    //staff
                    if (visualNovel.Staff.Count > 0)
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
                        dbVnInfoStaff.Upsert(vnStaff);
                    }


                    if (visualNovel.Tags.Count > 0)
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

                        dbVnInfoTags.Upsert(vnTags);
                    }
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

        public void FormatVnCharacters(List<Character> characters, uint vnid)
        {
            if(characters.Count <1)return; 
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbCharInfo = db.GetCollection<VnCharacterInfo>("vncharacter");
                var dbCharTraits = db.GetCollection<VnCharacterTraits>("vncharacter_traits");
                var dbCharVns = db.GetCollection<VnCharacterVns>("vncharacter_vns");
                var dbCharVoices = db.GetCollection<VnCharacterVoiced>("vncharacter_voiced");
                var dbCharInstances = db.GetCollection<VnCharacterInstances>("vncharacter_instances");

                if (characters.Count > 0)
                {
                    List<VnCharacterInfo> vnCharacters = new List<VnCharacterInfo>();
                    List<VnCharacterTraits> vnCharacterTraits = new List<VnCharacterTraits>();
                    List<VnCharacterVns> vnCharacterVns = new List<VnCharacterVns>();
                    List<VnCharacterVoiced> vnCharacterVoices = new List<VnCharacterVoiced>();
                    List<VnCharacterInstances> vnCharacterInstances = new List<VnCharacterInstances>();
                    foreach (Character vnCharacter in characters)
                    {
                        VnCharacterInfo character = new VnCharacterInfo()
                        {
                            VnId = vnid,
                            CharacterId = vnCharacter.Id,
                            Name = vnCharacter.Name,
                            Original = vnCharacter.OriginalName,
                            Gender = vnCharacter.Gender.ToString(),
                            BloodType = vnCharacter.BloodType.ToString(),
                            Birthday = BirthdayConverter.ConvertBirthday(vnCharacter.Birthday),
                            Aliases = CsvConverter.ConvertToCsv(vnCharacter.Aliases),
                            Description = vnCharacter.Description,
                            ImageLink = vnCharacter.Image,
                            Bust = Convert.ToInt32(vnCharacter.Bust),
                            Waist = Convert.ToInt32(vnCharacter.Waist),
                            Hip = Convert.ToInt32(vnCharacter.Hip),
                            Height = Convert.ToInt32(vnCharacter.Height),
                            Weight = Convert.ToInt32(vnCharacter.Weight)
                        };
                        vnCharacters.Add(character);

                        vnCharacterTraits.AddRange(vnCharacter.Traits.Select(traits => (new VnCharacterTraits()
                        {
                            CharacterId = character.CharacterId, TraitId = traits.Id, SpoilerLevel = traits.SpoilerLevel
                        })));

                        vnCharacterVns.AddRange(vnCharacter.VisualNovels.Select(vn => new VnCharacterVns()
                        {
                            CharacterId = vnCharacter.Id,
                            VnId = vn.Id,
                            ReleaseId = vn.ReleaseId,
                            SpoilerLevel = (byte) vn.SpoilerLevel,
                            Role = vn.Role.ToString()
                        }));

                        vnCharacterVoices.AddRange(vnCharacter.VoiceActorMetadata.Select(voice =>
                            new VnCharacterVoiced()
                            {
                                StaffId = voice.StaffId, StaffAliasId = voice.AliasId, VnId = voice.VisualNovelId,
                                Note = voice.Note
                            }));

                        vnCharacterInstances.AddRange(vnCharacter.CharacterInstances.Select(instance =>
                            new VnCharacterInstances()
                            {
                                CharacterId = (int)character.CharacterId, Spoiler = (byte) instance.Spoiler, Name = instance.Name,
                                Original = instance.Kanji
                            }));
                    }

                    dbCharInfo.Upsert(vnCharacters);
                    dbCharTraits.Upsert(vnCharacterTraits);
                    dbCharVns.Upsert(vnCharacterVns);
                    dbCharVoices.Upsert(vnCharacterVoices);
                    dbCharInstances.Upsert(vnCharacterInstances);
                }
            }

        }

        public void FormatVnReleases(List<Release> vnReleases, uint vnid)
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
                        var prevVnReleaseMedia = dbVnReleaseMedia.Query().Where(x => x.ReleaseId == vnRelease.Id);
                        foreach (var media in vnRelease.Media)
                        {
                            var entry = prevVnReleaseMedia.FirstOrDefault() ?? new VnReleaseMedia();
                            entry.ReleaseId = vnRelease.Id;
                            entry.Medium = media.Medium;
                            entry.Quantity = media.Quantity;
                            vnReleaseMediaList.Add(entry);
                        }
                    }


                    if (vnRelease.Producers.Count > 0)
                    {
                        var prevVnReleaseProducers =
                            dbVnReleaseProducers.Query().Where(x => x.ProducerId == vnRelease.Id);
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
                    }

                    if (vnRelease.VisualNovels.Count > 0)
                    {
                        var prevVnReleaseVns = dbReleaseVns.Query().Where(x => x.ReleaseId == vnRelease.Id);

                        foreach (var vn in vnRelease.VisualNovels)
                        {
                            var entry = prevVnReleaseVns.FirstOrDefault() ?? new VnReleaseVn();
                            entry.ReleaseId = vnRelease.Id;
                            entry.VnId = vn.Id;
                            entry.Name = vn.Name;
                            entry.Original = vn.OriginalName;
                            vnReleaseVnsList.Add(entry);

                        }
                    }

                }

                dbVnRelease.Upsert(vnReleaseList);
                dbVnReleaseMedia.Upsert(vnReleaseMediaList);
                dbVnReleaseProducers.Upsert(vnReleaseProducersList);
                dbReleaseVns.Upsert(vnReleaseVnsList);

            }

        }

        public void SaveProducers(List<Producer> vnProducers)
        {
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbVnProducer = db.GetCollection<VnProducer>("vnproducer");
                var dbVnProducerLinks = db.GetCollection<VnProducerLinks>("vnproducer_links");
                var dbVnProducerRelations = db.GetCollection<VnProducerRelations>("vnproducer_relations");
                
                List<VnProducer>vnProducersList = new List<VnProducer>();
                List<VnProducerLinks>vnProducerLinksList = new List<VnProducerLinks>();
                List<VnProducerRelations>vnProducerRelationsList = new List<VnProducerRelations>();
                foreach (var vnProducer in vnProducers)
                {
                    var prevVnProducers = dbVnProducer.Query().Where(x => x.ProducerId == vnProducer.Id);
                    var producer = prevVnProducers.FirstOrDefault() ?? new VnProducer();

                    producer.ProducerId = (int?) vnProducer.Id;
                    producer.Name = vnProducer.Name;
                    producer.Original = vnProducer.OriginalName;
                    producer.ProducerType = vnProducer.ProducerType;
                    producer.Language = vnProducer.Language;
                    producer.Aliases = CsvConverter.ConvertToCsv(vnProducer.Aliases);
                    producer.Description = vnProducer.Description;
                    vnProducersList.Add(producer);


                    var prevVnProducerLinks = dbVnProducerLinks.Query().Where(x => x.ProducerId == vnProducer.Id);
                    var links = prevVnProducerLinks.FirstOrDefault() ?? new VnProducerLinks();
                    links.ProducerId = (int) vnProducer.Id;
                    links.Homepage = vnProducer.Links.Homepage;
                    links.WikiData = vnProducer.Links.Wikipedia;
                    vnProducerLinksList.Add(links);

                    var prevVnProducerRelations =
                        dbVnProducerRelations.Query().Where(x => x.ProducerId == vnProducer.Id);

                    foreach (var relation in vnProducer.Relations)
                    {
                        var entry = prevVnProducerRelations.FirstOrDefault() ?? new VnProducerRelations();
                        entry.RelationId = (int?) relation.Id;
                        entry.ProducerId = (int?) vnProducer.Id;
                        entry.Relation = relation.Relation;
                        entry.Name = relation.Name;
                        entry.Original = relation.OriginalName;
                        vnProducerRelationsList.Add(entry);

                    }
                }
            }
        }

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

                    staff.StaffId = (int?) vnStaff.Id;
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
                        var prevVnStaffAliases = dbVnStaffAliases.Query().Where(x => x.StaffId == vnStaff.Id);
                        var vnAliases = prevVnStaffAliases.FirstOrDefault() ?? new VnStaffAliases();
                        foreach (var alias in vnStaff.Aliases)
                        {
                            vnAliases.StaffId = (int?) vnStaff.Id;
                            vnAliases.AliasId = (int) alias.Id;
                            vnAliases.Name = alias.Name;
                            vnAliases.Original = alias.OriginalName;
                            vnStaffAliasesList.Add(vnAliases);
                        }
                    }

                    //vns
                    if (vnStaff.Vns.Length > 0)
                    {
                        var prevVnStaffVns = dbVnStaffVns.Query().Where(x => x.StaffId == staff.StaffId);
                        var staffVns = prevVnStaffVns.FirstOrDefault() ?? new VnStaffVns();
                        foreach (var vn in vnStaff.Vns)
                        {
                            staffVns.VnId = vnid;
                            staffVns.StaffId = (int?) vnStaff.Id;
                            staffVns.AliasId = (int) staffVns.AliasId;
                            staffVns.Role = vn.Role;
                            staffVns.Note = vn.Note;
                            vnStaffVnList.Add(staffVns);
                        }
                    }
                    //voiced
                    if (vnStaff.Voiced.Length > 0)
                    {
                        var prevVnStaffVoiced = dbVnStaffVoiced.Query().Where(x => x.StaffId == vnStaff.Id);
                        foreach (var voiced in vnStaff.Voiced)
                        {
                            var entry = prevVnStaffVoiced.FirstOrDefault() ?? new VnStaffVoiced();
                            entry.VnId = vnid;
                            entry.StaffId = (int?)vnStaff.Id;
                            entry.CharacterId = (int)voiced.CharacterId;
                            entry.Note = voiced.Note;
                            vnStaffVoicedList.Add(entry);
                        }
                    }

                }

                dbVnStaff.Upsert(staffList);
                dbVnStaffAliases.Upsert(vnStaffAliasesList);
                dbVnStaffVns.Upsert(vnStaffVnList);
                dbVnStaffVoiced.Upsert(vnStaffVoicedList);
            }
        }
    }
}
