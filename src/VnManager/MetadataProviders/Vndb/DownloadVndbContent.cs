// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
using VndbSharp;
using VndbSharp.Models.Dumps;
using VnManager.Converters;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.ViewModels;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders.Vndb
{
    internal static class DownloadVndbContent
    {
        /// <summary>
        /// Download Vndb cover image
        /// </summary>
        /// <param name="vnId"></param>
        /// <returns></returns>
        internal static async Task DownloadCoverImageAsync(uint vnId)
        {
            try
            {
                RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownCoverImage");
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'");
                VnInfo entry = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().Where(x => x.VnId == vnId).FirstOrDefault();
                if (entry == null)
                {
                    return;
                }
                if (entry.ImageLink != null)
                {
                    var uri = new Uri(entry.ImageLink);
                    RootViewModel.StatusBarPage.IsFileDownloading = true;
                    string path = $@"{App.AssetDirPath}\sources\vndb\images\cover\{entry.VnId}.jpg";
                    await ImageHelper.DownloadImageAsync(uri, NsfwHelper.RawRatingIsNsfw(entry.ImageRating), path);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to download cover image");
                SentrySdk.CaptureException(ex);
            }
            finally
            {
                RootViewModel.StatusBarPage.IsFileDownloading = false;
                RootViewModel.StatusBarPage.InfoText = string.Empty;
            }
        }

        /// <summary>
        /// Download character images
        /// </summary>
        /// <param name="vnId"></param>
        /// <returns></returns>
        internal static async Task DownloadCharacterImagesAsync(uint vnId)
        {
            try
            {
                RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownCharImages");
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'");
                var entries = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query().Where(x => x.VnId == vnId)
                    .ToList();
                if (entries == null || entries.Count == 0)
                {
                    App.Logger.Warning("Failed to download character image. Entries is null or empty");
                    return;
                }

                var directory = Path.Combine(App.AssetDirPath, @$"sources\vndb\images\characters\{vnId}");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                List<string> characterList = entries.Select(x => x.ImageLink).ToList();
                using var client = new WebClient();
                foreach (var character in characterList)
                {
                    string file = $@"{directory}\{Path.GetFileName(character)}";
                    if (!File.Exists(file) && !string.IsNullOrEmpty(character))
                    {
                        RootViewModel.StatusBarPage.IsFileDownloading = true;
                        await client.DownloadFileTaskAsync(new Uri(character), file);
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to download character image");
                SentrySdk.CaptureException(e);
            }
            finally
            {
                RootViewModel.StatusBarPage.IsFileDownloading = false;
                RootViewModel.StatusBarPage.InfoText = string.Empty;
            }
        }

        /// <summary>
        /// Download Screenshots
        /// </summary>
        /// <param name="vnId"></param>
        /// <returns></returns>
        internal static async Task DownloadScreenshotsAsync(uint vnId)
        {
            try
            {
                RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownScreenshots");
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var entries = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString()).Query().Where(x => x.VnId == vnId)
                        .ToList();
                    if (entries == null || entries.Count == 0)
                    {
                        App.Logger.Warning("Failed to download screenshots. Entries is null or empty");
                        return;
                    }

                    var directory = Path.Combine(App.AssetDirPath, @$"sources\vndb\images\screenshots\{vnId}");
                    if (!Directory.Exists($@"{directory}\thumbs"))
                    {
                        Directory.CreateDirectory($@"{directory}\thumbs");
                    }
                    List<BindingImage> scrList = entries.Select(screen => 
                        new BindingImage { IsNsfw = NsfwHelper.RawRatingIsNsfw(screen.ImageRating), ImageLink = screen.ImageLink, Rating = screen.ImageRating}).ToList();

                    RootViewModel.StatusBarPage.IsFileDownloading = true;
                    await ImageHelper.DownloadImagesWithThumbnailsAsync(scrList, directory);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to download screenshots");
                SentrySdk.CaptureException(ex);
                throw;
            }
            finally
            {
                RootViewModel.StatusBarPage.IsFileDownloading = false;
                RootViewModel.StatusBarPage.InfoText = string.Empty;
            }
        }

        //Tag and Trait Dumps
        
        /// <summary>
        /// Save Tag Dump
        /// </summary>
        /// <returns></returns>
        public static async Task GetAndSaveTagDumpAsync()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }

                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    RootViewModel.StatusBarPage.IsWorking = true;
                    var dbTags = db.GetCollection<VnTagData>(DbVnDump.VnDump_TagData.ToString());
                    RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownTagDump");
                    RootViewModel.StatusBarPage.IsDatabaseProcessing = true;
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
                SentrySdk.CaptureException(ex);
                StatusBarViewModel.ResetValues();
            }
            finally
            {
                RootViewModel.StatusBarPage.IsDatabaseProcessing = false;
                RootViewModel.StatusBarPage.InfoText = string.Empty;
            }
        }

        /// <summary>
        /// Save TraitDump to db
        /// </summary>
        /// <returns></returns>
        public static async Task GetAndSaveTraitDumpAsync()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }

                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    RootViewModel.StatusBarPage.IsWorking = true;
                    var dbTraits = db.GetCollection<VnTraitData>(DbVnDump.VnDump_TraitData.ToString());
                    RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownTraitDump");
                    RootViewModel.StatusBarPage.IsDatabaseProcessing = true;
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
                        entry.IsSearchable = item.IsSearchable;
                        entry.IsApplicable = item.IsApplicable;
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
                SentrySdk.CaptureException(ex);
                StatusBarViewModel.ResetValues();
                throw;
            }
            finally
            {
                RootViewModel.StatusBarPage.IsDatabaseProcessing = false;
                RootViewModel.StatusBarPage.InfoText = string.Empty;
            }
        }
    }
}
