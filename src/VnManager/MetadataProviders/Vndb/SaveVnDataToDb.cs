using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LiteDB;
using VndbSharp.Models.VisualNovel;
using VnManager.Converters;
using VnManager.Models.Db.Vndb.Main;

namespace VnManager.MetadataProviders.Vndb
{
    public class SaveVnDataToDb
    {

        public void SaveVnInfo(VisualNovel visualNovel)
        {
            if (visualNovel == null)
            {
                return;
            }
            try
            {
                using (var db = new LiteDatabase(Path.Combine(App.ConfigDirPath, @"database\Data.db")))
                {
                    var dbVnInfo = db.GetCollection<VnInfo>("vninfo_main");
                    var dbVnInfoAnime = db.GetCollection<VnInfoAnime>("vninfo_anime");
                    var dbVnInfoLinks = db.GetCollection<VnInfoLinks>("vninfo_links");
                    var dbVnInfoScreens = db.GetCollection<VnInfoScreens>("vninfo_screens");
                    var dbVnInfoRelations = db.GetCollection<VnInfoRelations>("vninfo_relations");
                    var dbVnInfoStaff = db.GetCollection<VnInfoStaff>("vninfo_staff");
                    var dbVnInfoTags = db.GetCollection<VnInfoTags>("vninfo_tags");
                    var vn = new VnInfo
                    {
                        VnId = visualNovel.Id,
                        Title = visualNovel.Name,
                        Original = visualNovel.OriginalName,
                        Released = visualNovel.Released?.ToString() ?? null,
                        Languages = CsvConverter.ConvertToCsv(visualNovel.Languages),
                        OriginalLanguages = CsvConverter.ConvertToCsv(visualNovel.OriginalLanguages),
                        Platforms = CsvConverter.ConvertToCsv(visualNovel.Platforms),
                        Aliases = CsvConverter.ConvertToCsv(visualNovel.Aliases),
                        Length = visualNovel.Length?.ToString(),
                        Description = visualNovel.Description,
                        ImageLink = visualNovel.Image,
                        ImageNsfw = visualNovel.IsImageNsfw,
                        Popularity = visualNovel.Popularity,
                        Rating = visualNovel.Rating
                    };
                    dbVnInfo.Upsert(vn);
                    dbVnInfo.EnsureIndex(x => x.Index);

                    //anime
                    List<VnInfoAnime> vnAnime = visualNovel.Anime.Select(anime => new VnInfoAnime()
                        {
                            VnId = visualNovel.Id,
                            AniDbId = anime.AniDbId,
                            AnnId = anime.AnimeNewsNetworkId,
                            AniNfoId = anime.AnimeNfoId,
                            TitleEng = anime.RomajiTitle,
                            TitleJpn = anime.KanjiTitle,
                            Year = anime.AiringYear?.ToString() ?? null,
                            AnimeType = anime.Type
                        })
                        .ToList();
                    dbVnInfoAnime.Upsert(vnAnime);
                    dbVnInfoAnime.EnsureIndex(x => x.Index);

                    //links
                    VnInfoLinks vnLinks = new VnInfoLinks()
                    {
                        VnId = visualNovel.Id,
                        Wikipedia = visualNovel.VisualNovelLinks.Wikipedia,
                        Encubed = visualNovel.VisualNovelLinks.Encubed,
                        Renai = visualNovel.VisualNovelLinks.Renai
                    };
                    dbVnInfoLinks.Upsert(vnLinks);
                    dbVnInfoLinks.EnsureIndex(x => x.Index);

                    //screenshot
                    if (visualNovel.Screenshots.Count <= 0) return;
                    List<VnInfoScreens> vnScreenshots = visualNovel.Screenshots.Select(screenshot => new VnInfoScreens()
                        {
                            VnId = visualNovel.Id,
                            ImageUrl = screenshot.Url,
                            ReleaseId = screenshot.ReleaseId,
                            Nsfw = screenshot.IsNsfw,
                            Height = screenshot.Height,
                            Width = screenshot.Width
                        })
                        .ToList();
                    dbVnInfoScreens.Upsert(vnScreenshots);
                    dbVnInfoScreens.EnsureIndex(x => x.Index);

                    //relations
                    if (visualNovel.Relations.Count > 0)
                    {
                        List<VnInfoRelations> vnRelations = visualNovel.Relations.Select(relation => new VnInfoRelations()
                            {
                                VnId = visualNovel.Id,
                                RelationId = relation.Id,
                                Relation = relation.Type.ToString(),
                                Title = relation.Title,
                                Original = relation.Original,
                                Official = relation.Official ? "Yes" : "No"
                            })
                            .ToList();

                        dbVnInfoRelations.Upsert(vnRelations);
                        dbVnInfoRelations.EnsureIndex(x => x.Index);
                    }

                    //staff
                    if (visualNovel.Staff.Count > 0)
                    {
                        List<VnInfoStaff> vnStaff = visualNovel.Staff.Select(staff => new VnInfoStaff()
                            {
                                VnId = visualNovel.Id,
                                StaffId = staff.StaffId,
                                AliasId = staff.AliasId,
                                Name = staff.Name,
                                Original = staff.Kanji,
                                Role = staff.Role,
                                Note = staff.Note
                            })
                            .ToList();

                        dbVnInfoStaff.Upsert(vnStaff);
                        dbVnInfoStaff.EnsureIndex(x => x.Index);
                    }

                    //tags
                    if (visualNovel.Tags.Count <= 0) return;
                    List<VnInfoTags> vnTags = visualNovel.Tags.Select(tag => new VnInfoTags() {VnId = visualNovel.Id, TagId = tag.Id, Score = tag.Score, Spoiler = tag.SpoilerLevel}).ToList();
                    dbVnInfoTags.Upsert(vnTags);
                    dbVnInfoTags.EnsureIndex(x => x.Index);
                }

            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Something happened");
            }
            
        }
    }
}
