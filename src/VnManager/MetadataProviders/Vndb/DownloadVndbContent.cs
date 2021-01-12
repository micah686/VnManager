using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using VndbSharp;
using VndbSharp.Models.Dumps;
using VnManager.Converters;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders.Vndb
{
    internal static class DownloadVndbContent
    {
        internal static async Task DownloadCoverImageAsync(uint vnId)
        {
            try
            {
                App.StatusBar.InfoText = App.ResMan.GetString("DownCoverImage");
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                VnInfo entry = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().Where(x => x.VnId == vnId).FirstOrDefault();
                if (entry == null)
                {
                    return;
                }
                if (entry.ImageLink != null)
                {
                    var uri = new Uri(entry.ImageLink);
                    App.StatusBar.IsFileDownloading = true;
                    string path = $@"{App.AssetDirPath}\sources\vndb\images\cover\{Path.GetFileName(uri.AbsoluteUri)}";
                    await ImageHelper.DownloadImageAsync(uri, NsfwHelper.RawRatingIsNsfw(entry.ImageRating), path);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to download cover image");
            }
            finally
            {
                App.StatusBar.IsFileDownloading = false;
            }
        }

        internal static async Task DownloadCharacterImagesAsync(uint vnId)
        {
            try
            {
                App.StatusBar.InfoText = App.ResMan.GetString("DownCharImages");
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                var entries = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString()).Query().Where(x => x.VnId == vnId)
                    .ToList();
                if (entries == null || entries.Count == 0)
                {
                    App.Logger.Warning("Failed to download character image. Entries is null or empty");
                    return;
                }

                var directory = Path.Combine(App.AssetDirPath, @$"sources\vndb\images\characters\{vnId}");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                List<string> characterList = entries.Select(x => x.ImageLink).ToList();
                using var client = new WebClient();
                foreach (var character in characterList)
                {
                    string file = $@"{directory}\{Path.GetFileName(character)}";
                    if (!File.Exists(file) && !string.IsNullOrEmpty(character))
                    {
                        App.StatusBar.IsFileDownloading = true;
                        await client.DownloadFileTaskAsync(new Uri(character), file);
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to download character image");
            }
            finally
            {
                App.StatusBar.IsFileDownloading = false;
            }
        }

        internal static async Task DownloadScreenshotsAsync(uint vnId)
        {
            try
            {
                App.StatusBar.InfoText = App.ResMan.GetString("DownScreenshots");
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
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
                    List<BindingImage> scrList = entries.Select(screen => new BindingImage { IsNsfw = NsfwHelper.RawRatingIsNsfw(screen.ImageRating), ImageLink = screen.ImageLink, Rating = screen.ImageRating}).ToList();

                    App.StatusBar.IsFileDownloading = true;
                    await ImageHelper.DownloadImagesWithThumbnailsAsync(scrList, directory);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to download screenshots");
                throw;
            }
            finally
            {
                App.StatusBar.IsFileDownloading = false;
            }
        }

        //Tag and Trait Dumps
        public static async Task GetAndSaveTagDumpAsync()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    App.StatusBar.IsWorking = true;
                    var dbTags = db.GetCollection<VnTagData>(DbVnDump.VnDump_TagData.ToString());
                    App.StatusBar.InfoText = App.ResMan.GetString("DownTagDump");
                    List<Tag> tagDump = (await VndbUtils.GetTagsDumpAsync()).ToList();
                    App.StatusBar.IsDatabaseProcessing = true;
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
                    App.StatusBar.IsDatabaseProcessing = false;
                    App.StatusBar.InfoText = "";
                    App.StatusBar.IsWorking = false;
                    App.StatusBar.StatusString = App.ResMan.GetString("Ready");
                }

            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "An error happened while getting/saving the tag dump");
                StatusBarViewModel.ResetValues();
            }
        }

        public static async Task GetAndSaveTraitDumpAsync()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    App.StatusBar.IsWorking = true;
                    var dbTraits = db.GetCollection<VnTraitData>(DbVnDump.VnDump_TraitData.ToString());
                    App.StatusBar.InfoText = App.ResMan.GetString("DownTraitDump");
                    List<Trait> traitDump = (await VndbUtils.GetTraitsDumpAsync()).ToList();
                    App.StatusBar.IsDatabaseProcessing = true;
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
                    App.StatusBar.IsDatabaseProcessing = false;
                    App.StatusBar.InfoText = "";
                    App.StatusBar.IsWorking = false;
                    App.StatusBar.StatusString = App.ResMan.GetString("Ready");
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "An error happened while getting/saving the trait dump");
                StatusBarViewModel.ResetValues();
                throw;
            }
        }
    }
}
