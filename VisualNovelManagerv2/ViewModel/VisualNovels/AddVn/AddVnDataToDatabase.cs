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
using System.Security.Policy;
using VisualNovelManagerv2.EF.Entity.VnCharacter;

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
                            await GetDetailsFromTagDump(visualNovel.Tags);
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


                    #region VnCharacter

                    foreach (Character character in characters)
                    {
                        #region VnCharacter
                        context.VnCharacter.Add(new EF.Entity.VnCharacter.VnCharacter
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
                        await GetDetailsFromTraitDump(character.Traits, character.Id);
                        //IEnumerable<Trait> traitMatches = GetDetailsFromTraitDump(traitDump, character.Traits);
                        //int count = 0;
                        //foreach (TraitMetadata trait in character.Traits)
                        //{
                        //    if (traitMatches.Any(c => c.Id == trait.Id))//prevents crashes with awaiting traits
                        //    {
                        //        vncharactertraits.Add(new VnCharacterTraits
                        //        {
                        //            CharacterId = Convert.ToInt32(character.Id),
                        //            TraitId = Convert.ToInt32(trait.Id),
                        //            TraitName = traitMatches.ElementAt(count).Name,
                        //            SpoilerLevel = trait.SpoilerLevel.ToString() ?? null,
                        //        });
                        //        count++;
                        //    }
                        //}

                        #endregion

                        #region TraitMatches
                        //if (traitMatches != null)
                        //{
                        //    foreach (Trait trait in traitMatches)
                        //    {
                        //        vntraitdata.Add(new VnTraitData
                        //        {
                        //            TraitId = Convert.ToInt32(trait.Id),
                        //            Name = trait.Name,
                        //            Description = trait.Description,
                        //            Meta = trait.IsMeta.ToString(),
                        //            Chars = Convert.ToInt32(trait.Characters),
                        //            Aliases = ConvertToCsv(trait.Aliases),
                        //            Parents = string.Join(",", trait.Parents)
                        //        });
                        //    }
                        //}


                        #endregion

                        #region VnCharacterVns
                        foreach (VndbSharp.Models.Character.VisualNovelMetadata vn in character.VisualNovels)
                        {
                            context.VnCharacterVns.Add(new VnCharacterVns
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

                    #endregion

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

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }




        private async Task GetDetailsFromTagDump(ReadOnlyCollection<TagMetadata> vnTags)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    // this query SHOULD be implementing this: foreach (TagMetadata tag in visualNovel.Tags){if (tagMatches.Any(c => c.Id == tag.Id)){} }
                    //checks if the dump was downloaded, or if 24 hours have passed
                    if (_didDownloadTagDump.Key == false || Math.Abs(_didDownloadTagDump.Value.Subtract(DateTime.Now).TotalHours) >= 24)
                    {
                        #region This section deals with the daily TagDump ONLY

                        List<Tag> tagDump = (await VndbUtils.GetTagsDumpAsync()).ToList();
                        List<VnTagData> tagsToAdd = tagDump.Select(tag => new VnTagData
                        {
                            TagId = tag.Id,
                            Name = tag.Name,
                            Description = tag.Description,
                            Meta = tag.IsMeta.ToString(),
                            Vns = tag.VisualNovels,
                            Cat = tag.TagCategory.ToString(),
                            Aliases = ConvertToCsv(tag.Aliases),
                            Parents = tag.Parents != null ? string.Join(",", tag.Parents) : null,
                        }).ToList();

                        //IQueryable<VnTagData> foo = context.VnTagData.Where(x => tagsToAdd.Any(y => y.TagId == x.TagId));
                        //tags that AREN'T exact duplicates, that also share the same ID (contents edited online, ID wasn't)
                        //TODO: check/fix the EXCEPT method
                        List<VnTagData> tagsToDelete = context.VnTagData.Except(tagsToAdd).Where(x => tagsToAdd.Any(y => y.TagId == x.TagId)).ToList();
                        context.RemoveRange(tagsToDelete);
                        context.VnTagData.AddRange(tagsToAdd);
                        #endregion This section deals with the daily TagDump ONLY

                        #region This Section is for VnInfoTags
                        //gets a list of all tags from the TagDump where the dump contains the tagIds from the vn
                        List<TagMetadata> vnInfoTags = (from tag in vnTags from ef in tagDump where tag.Id == ef.Id select tag).ToList();

                        List<VnInfoTags> vnInfoTagsToAdd = vnInfoTags.Select(tagMetadata => new VnInfoTags
                        {
                            VnId = _vnid,
                            TagId = tagMetadata.Id,
                            Score = tagMetadata.Score,
                            Spoiler = tagMetadata.SpoilerLevel.ToString()
                        }).ToList();

                        //list of items to delete where the db DOESN'T contain the exact item from tagsToAdd (indicates something was modified)
                        List<VnInfoTags> vnInfoTagsToDelete = context.VnInfoTags.Where(x => !vnInfoTagsToAdd.Any(y => y.VnId == x.VnId &&
                            Convert.ToDecimal(y.Score) == Convert.ToDecimal(x.Score) && y.Spoiler == x.Spoiler && y.TagId == x.TagId)).ToList();

                        context.VnInfoTags.RemoveRange(vnInfoTagsToDelete);

                        //removes all items from the ItemsToAdd where the vnId and TagId already exists in the database
                        vnInfoTagsToAdd.RemoveAll(x => vnInfoTagsToAdd.Where(item => context.VnInfoTags.Where(v => v.VnId == _vnid).Any(y => y.TagId == item.TagId)).Contains(x));

                        context.VnInfoTags.AddRange(vnInfoTagsToAdd);

                        context.SaveChanges();

                        #endregion End This Section is for VnInfoTags

                        _didDownloadTagDump.Key = true;
                        //gets a list of items from VnTagData where it contains the tagId from the vn
                        //List<VnTagData> matches = (from ef in context.VnTagData from tag in vnTags where ef.TagId == tag.Id select ef).ToList();
                    }
                    else
                    {
                        //gets a list of all tags from the VnTagData where that data contains the tagId from the vn
                        List<TagMetadata> vnInfoTags = (from tag in vnTags from ef in context.VnTagData where tag.Id == ef.TagId select tag).ToList();
                        List<VnInfoTags> vnInfoTagsToAdd = vnInfoTags.Select(tagMetadata => new VnInfoTags
                        {
                            VnId = _vnid,
                            TagId = tagMetadata.Id,
                            Score = tagMetadata.Score,
                            Spoiler = tagMetadata.SpoilerLevel.ToString()
                        }).ToList();
                        //list of items to delete where the db DOESN'T contain the exact item from tagsToAdd (indicates something was modified)
                        List<VnInfoTags> vnInfoTagsToDelete = context.VnInfoTags.Where(x => !vnInfoTagsToAdd.Any(y => y.VnId == x.VnId &&
                             Convert.ToDecimal(y.Score) == Convert.ToDecimal(x.Score) && y.Spoiler == x.Spoiler && y.TagId == x.TagId)).ToList();

                        context.VnInfoTags.RemoveRange(vnInfoTagsToDelete);

                        //removes all items from the ItemsToAdd where the vnId and TagId already exists in the database
                        vnInfoTagsToAdd.RemoveAll(x => vnInfoTagsToAdd.Where(item => context.VnInfoTags.Where(v => v.VnId == _vnid).Any(y => y.TagId == item.TagId)).Contains(x));

                        context.VnInfoTags.AddRange(vnInfoTagsToAdd);

                        context.SaveChanges();
                        
                        //for selection. Remove once I use this elsewhere
                        //gets a list of items from VnTagData where it contains the tagId from the vn
                        //List<VnTagData> matches = (from ef in context.VnTagData from tag in vnTags where ef.TagId == tag.Id select ef).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }                        
        }

        private async Task GetDetailsFromTraitDump(ReadOnlyCollection<TraitMetadata> traits, UInt32 charId)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    //checks if the dump was downloaded, or if 24 hours have passed
                    if (_didDownloadTraitDump.Key == false || Math.Abs(_didDownloadTraitDump.Value.Subtract(DateTime.Now).TotalHours) >= 24)
                    {
                        #region This section deals with the daily TraitDump ONLY
                        List<Trait> traitDump = (await VndbUtils.GetTraitsDumpAsync()).ToList();
                        List<VnTraitData> traitsToAdd = traitDump.Select(trait => new VnTraitData
                        {
                            TraitId = trait.Id,
                            Name = trait.Name,
                            Description = trait.Description,
                            Meta = trait.IsMeta.ToString(),
                            Chars = trait.Characters,
                            Aliases = ConvertToCsv(trait.Aliases),
                            Parents = trait.Parents != null ? string.Join(",", trait.Parents) : null
                        }).ToList();

                        //TODO: FIx this so it only finds the modified items, and also deletes from the collection properly
                        //traits that AREN'T exact duplicates, that also share the same ID (contents edited online, ID wasn't)
                        List<VnTraitData> traitsToDelete = context.VnTraitData.Where(x => traitsToAdd.Any(y => y.TraitId == x.TraitId && y.Aliases == x.Aliases &&
                        y.Chars == x.Chars && y.Description == x.Description && y.Meta == x.Meta && y.Name == x.Name && y.Parents == x.Parents)).ToList();
                        
                        traitsToAdd.RemoveAll(x => traitsToDelete.Contains(x));

                        //context.RemoveRange(traitsToDelete);
                        context.VnTraitData.AddRange(traitsToAdd);
                        #endregion This section deals with the daily TraitDump ONLY

                        #region This Section is for VnCharacterTraits
                        //gets a list of all traits from the TraitDump where the dump contains the traitIds from the character
                        List<TraitMetadata> vnCharacterTraits = (from trait in traits from ef in traitDump where trait.Id == ef.Id select trait).ToList();
                        var vnCharacterTraitsToAdd = vnCharacterTraits.Select(traitMetaData => new VnCharacterTraits
                        {
                            CharacterId = charId,
                            TraitId = traitMetaData.Id,
                            SpoilerLevel = traitMetaData.SpoilerLevel.ToString()
                        }).ToList();

                        //list of items to delete where the db DOESN'T contain the exact item from traitsToAdd (indicates something was modified)
                        List<VnCharacterTraits> vnCharacterTraitsToDelete = context.VnCharacterTraits.Where(x => !vnCharacterTraitsToAdd.Any(y => y.CharacterId == x.CharacterId &&
                             y.SpoilerLevel == x.SpoilerLevel && y.TraitId == x.TraitId)).ToList();

                        context.VnCharacterTraits.RemoveRange(vnCharacterTraitsToDelete);
                        //removes all items from the ItemsToAdd where the vnId and TraitId already exists in the database
                        vnCharacterTraitsToAdd.RemoveAll(x => vnCharacterTraitsToAdd.Where(item => context.VnCharacterTraits.Where(c => c.CharacterId == charId)
                                .Any(y => y.TraitId == item.TraitId)).Contains(x));

                        context.VnCharacterTraits.AddRange(vnCharacterTraitsToAdd);
                        context.SaveChanges();
                        _didDownloadTraitDump.Key = true;

                        #endregion
                    }
                    else
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }

            //IEnumerable<Trait> matches = from traitMetadata in traits from tTrait in traitDump where tTrait.Id == traitMetadata.Id select tTrait;
            
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
