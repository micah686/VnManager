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
                List<VnRelease> vnRelease = new List<VnRelease>();
                List<VnReleaseMedia> vnReleaseMedia = new List<VnReleaseMedia>();
                List<VnReleaseProducers> vnReleaseProducers = new List<VnReleaseProducers>();
                List<VnReleaseVn> vnReleaseVns = new List<VnReleaseVn>();
                foreach (Release release in vnReleases)
                {
                    vnRelease.Add(new VnRelease()
                    {
                        ReleaseId = release.Id,
                        Title = release.Name,
                        Original = release.OriginalName,
                        ReleaseType = release.Type.ToString(),
                        Patch = release.IsPatch,
                        Freeware = release.IsFreeware,
                        Doujin = release.IsDoujin,
                        Languages = CsvConverter.ConvertToCsv(release.Languages),
                        Website = release.Website,
                        Notes = release.Notes,
                        MinAge = Convert.ToByte(release.MinimumAge),
                        Gtin = release.Gtin,
                        Catalog = release.Catalog,
                        Platforms = CsvConverter.ConvertToCsv(release.Platforms),
                        Resolution = release.Resolution,
                        Voiced = release.Voiced.ToString(),
                        Animation = string.Join(",", release.Animation)
                    });

                    if (release.Media.Count > 0)
                    {
                        vnReleaseMedia.AddRange(release.Media.Select(media => new VnReleaseMedia() 
                            {ReleaseId = release.Id, Medium = media.Medium, Quantity = media.Quantity}));
                    }

                    if (release.Producers.Count > 0)
                    {
                        vnReleaseProducers.AddRange(release.Producers.Select(producer => new VnReleaseProducers()
                        {
                            ReleaseId = release.Id,
                            ProducerId = producer.Id,
                            Developer = producer.IsDeveloper,
                            Publisher = producer.IsPublisher,
                            Name = producer.Name,
                            Original = producer.OriginalName,
                            ProducerType = producer.ProducerType
                        }));
                    }

                    if (release.VisualNovels.Count <= 0) continue;
                    vnReleaseVns.AddRange(release.VisualNovels.Select(vn => new VnReleaseVn() 
                        {ReleaseId = vn.Id, VnId = vnid, Name = vn.Name, Original = vn.OriginalName}));
                }

                dbVnRelease.Upsert(vnRelease);
                dbVnReleaseMedia.Upsert(vnReleaseMedia);
                dbVnReleaseProducers.Upsert(vnReleaseProducers);
                dbReleaseVns.Upsert(vnReleaseVns);

            }

        }

        public void SaveProducers(List<Producer> vnProducers)
        {
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbVnProducer = db.GetCollection<VnProducer>("vnproducer");
                var dbVnProducerLinks = db.GetCollection<VnProducerLinks>("vnproducer_links");
                var dbVnProducerRelations = db.GetCollection<VnProducerRelations>("vnproducer_relations");
                
                List<VnProducerRelations>vnProducerRelations = new List<VnProducerRelations>();
                foreach (var vnProducer in vnProducers)
                {
                    VnProducer producer = new VnProducer()
                    {
                        ProducerId = (int?)vnProducer.Id,
                        Name = vnProducer.Name,
                        Original = vnProducer.OriginalName,
                        ProducerType = vnProducer.ProducerType,
                        Language = vnProducer.Language,
                        Aliases = CsvConverter.ConvertToCsv(vnProducer.Aliases),
                        Description = vnProducer.Description,
                    };
                    dbVnProducer.Upsert(producer);

                    VnProducerLinks links = new VnProducerLinks()
                    {
                        ProducerId = (int?)vnProducer.Id,
                        Homepage = vnProducer.Links.Homepage,
                        WikiData = vnProducer.Links.Wikipedia
                    };
                    dbVnProducerLinks.Upsert(links);

                    vnProducerRelations.AddRange(vnProducer.Relations.Select(relation => new VnProducerRelations()
                    {
                        RelationId = (int?) relation.Id,
                        ProducerId = (int?) vnProducer.Id,
                        Relation = relation.Relation,
                        Name = relation.Name,
                        Original = relation.OriginalName
                    }));
                    dbVnProducerRelations.Upsert(vnProducerRelations);
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
                    VnStaff staff = new VnStaff()
                    {
                        StaffId = (int?)vnStaff.Id,
                        Name = vnStaff.Name,
                        Original = vnStaff.OriginalName,
                        Gender = vnStaff.Gender,
                        Language = vnStaff.Language,
                        VnStaffLinks = new VnStaffLinks()
                        {
                            Homepage = vnStaff.StaffLinks.Homepage,
                            Twitter = vnStaff.StaffLinks.Twitter,
                            AniDb = vnStaff.StaffLinks.AniDb,
                            Pixiv = vnStaff.StaffLinks.Pixiv,
                            Wikidata = vnStaff.StaffLinks.WikiData
                        },
                        Description = vnStaff.Description,
                        MainAliasId = vnStaff.MainAlias
                    };
                    staffList.Add(staff);

                    vnStaffAliasesList.AddRange(vnStaff.Aliases.Select(staffAliases => new VnStaffAliases()
                        { StaffId = (int?)vnStaff.Id, AliasId = (int)staffAliases.Id, Name = staffAliases.Name, Original = staffAliases.OriginalName }));

                    if (vnStaff.Vns.Length > 0)
                    {
                        vnStaffVnList.AddRange(vnStaff.Vns.Select(staffVns => new VnStaffVns()
                        {
                            VnId = vnid,
                            StaffId = (int?)vnStaff.Id,
                            AliasId = (int)staffVns.AliasId,
                            Role = staffVns.Role,
                            Note = staffVns.Note
                        }));
                    }

                    if (vnStaff.Voiced.Length > 0)
                    {
                        vnStaffVoicedList.AddRange(vnStaff.Voiced.Select(voices => new VnStaffVoiced()
                        {
                            VnId = vnid,
                            StaffId = (int?)vnStaff.Id,
                            AliasId = (int)voices.AliasId,
                            CharacterId = (int)voices.CharacterId,
                            Note = voices.Note
                        }));
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
