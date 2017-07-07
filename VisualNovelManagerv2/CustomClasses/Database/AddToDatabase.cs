using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.EntityFramework;
using VisualNovelManagerv2.EntityFramework.Entity.VnCharacter;
using VisualNovelManagerv2.EntityFramework.Entity.VnInfo;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VndbSharp;
using VndbSharp.Interfaces;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Common;
using VndbSharp.Models.Dumps;
using VndbSharp.Models.Errors;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;
using static System.Globalization.CultureInfo;
using VisualNovelMetadata = VndbSharp.Models.Release.VisualNovelMetadata;
using VisualNovelManagerv2.EntityFramework.Entity.VnOther;
using VisualNovelManagerv2.EntityFramework.Entity.VnRelease;
using VisualNovelManagerv2.EntityFramework.Entity.VnTagTrait;

namespace VisualNovelManagerv2.CustomClasses.Database
{
    public class AddToDatabase
    {
        private int _vnid;
        private uint _uvnid;
        private string _exepath;
        private string _iconpath;
        private double ProgressIncrement = 0;

        public async void GetId(int id, string exe, string icon)
        {
            _vnid = id;
            _uvnid = Convert.ToUInt32(id);
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
            using (Vndb client = new Vndb(true).WithClientDetails(Globals.ClientInfo[0], Globals.ClientInfo[1]))
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
                        VndbResponse<Character> characterList =
                            await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.Basic, ro);
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

                    VndbResponse<VisualNovel> visualNovels =
                        await client.GetVisualNovelAsync(VndbFilters.Id.Equals(_uvnid), VndbFlags.FullVisualNovel);
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage =
                            (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;

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
                            (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;

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
                            (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    #endregion

                    //await Task.Run((() => AddDataToDb(visualNovels, releases, characters)));
                    await Task.Run((() => AddDbTest(visualNovels, characters, releases)));
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
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                SQLiteTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    string sql = string.Empty;
                    SQLiteCommand cmd = null;

                    #region VnInfo Entity Framework
                    foreach (VisualNovel visualNovel in visualNovels)
                    {
                        #region VnInfo

                        sql = "INSERT OR REPLACE INTO VnInfo VALUES(@PK_Id, @VnId, @Title, @Original, @Released, @Languages, @OriginalLanguage, @Platforms, @Aliases, @Length," +
                              " @Description, @ImageLink, @ImageNsfw, @Popularity, @Rating);";
                        cmd = new SQLiteCommand(sql, connection, transaction);
                        cmd.Parameters.AddWithValue("@PK_Id", null);
                        cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovel.Id));
                        cmd.Parameters.AddWithValue("@Title", CheckForDbNull(visualNovel.Name));
                        cmd.Parameters.AddWithValue("@Original", CheckForDbNull(visualNovel.OriginalName));
                        cmd.Parameters.AddWithValue("@Released",
                            CheckForDbNull(visualNovel.Released?.ToString() ?? null));
                        cmd.Parameters.AddWithValue("@Languages",
                            CheckForDbNull(ConvertToCsv(visualNovel.Languages)));
                        cmd.Parameters.AddWithValue("@OriginalLanguage",
                            CheckForDbNull(ConvertToCsv(visualNovel.OriginalLanguages)));
                        cmd.Parameters.AddWithValue("@Platforms",
                            CheckForDbNull(ConvertToCsv(visualNovel.Platforms)));
                        cmd.Parameters.AddWithValue("@Aliases", CheckForDbNull(ConvertToCsv(visualNovel.Aliases)));
                        cmd.Parameters.AddWithValue("@Length", CheckForDbNull(visualNovel.Length.Value.ToString()));
                        cmd.Parameters.AddWithValue("@Description", CheckForDbNull(visualNovel.Description));
                        cmd.Parameters.AddWithValue("@ImageLink", CheckForDbNull(visualNovel.Image));
                        cmd.Parameters.AddWithValue("@ImageNsfw",
                            CheckForDbNull(visualNovel.IsImageNsfw.ToString()));
                        cmd.Parameters.AddWithValue("@Popularity", CheckForDbNull(visualNovel.Popularity));
                        cmd.Parameters.AddWithValue("@Rating", CheckForDbNull(visualNovel.Rating));

                        cmd.ExecuteNonQuery();

                        #endregion

                        #region VnInfoLinks

                        sql =
                            "INSERT OR REPLACE INTO VnInfoLinks VALUES(@PK_Id, @VnId, @Wikipedia, @Encubed, @Renai)";
                        cmd = new SQLiteCommand(sql, connection, transaction);
                        cmd.Parameters.AddWithValue("@PK_Id", null);
                        cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovel.Id));
                        cmd.Parameters.AddWithValue("@Wikipedia", visualNovel.VisualNovelLinks.Wikipedia);
                        cmd.Parameters.AddWithValue("@Encubed",
                            CheckForDbNull(visualNovel.VisualNovelLinks.Encubed));
                        cmd.Parameters.AddWithValue("@Renai", CheckForDbNull(visualNovel.VisualNovelLinks.Renai));
                        cmd.ExecuteNonQuery();

                        #endregion

                        #region VnAnime

                        if (visualNovel.Anime.Count > 0)
                        {
                            foreach (AnimeMetadata anime in visualNovel.Anime)
                            {
                                sql =
                                    "INSERT OR REPLACE INTO VnInfoAnime VALUES(@PK_Id, @VnId, @AniDbId, @AnnId, @AniNfoId, @TitleEng, @TitleJpn, @Year, @AnimeType);";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovel.Id));
                                cmd.Parameters.AddWithValue("@AniDbId", CheckForDbNull(anime.AniDbId));
                                cmd.Parameters.AddWithValue("@AnnId", CheckForDbNull(anime.AnimeNewsNetworkId));
                                cmd.Parameters.AddWithValue("@AniNfoId", CheckForDbNull(anime.AnimeNfoId));
                                cmd.Parameters.AddWithValue("@TitleEng", CheckForDbNull(anime.RomajiTitle));
                                cmd.Parameters.AddWithValue("@TitleJpn", CheckForDbNull(anime.KanjiTitle));
                                cmd.Parameters.AddWithValue("@Year",
                                    CheckForDbNull(anime.AiringYear?.ToString() ?? null));
                                cmd.Parameters.AddWithValue("@AnimeType", CheckForDbNull(anime.Type));
                                cmd.ExecuteNonQuery();
                            }
                        }


                        #endregion

                        #region VnTags

                        IEnumerable<Tag> tagMatches = null;
                        if (visualNovel.Tags.Count > 0)
                        {
                            tagMatches = await GetDetailsFromTagDump(visualNovel.Tags);
                            if (Globals.StatusBar.ProgressPercentage != null)
                                Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                            int count = 0;
                            foreach (TagMetadata tag in visualNovel.Tags)
                            {
                                if (tagMatches.Any(c => c.Id == tag.Id))//makes sure the matches contains tagid before proceeding, prevents crashing with tags waiting for approval
                                {
                                    sql = "INSERT OR REPLACE INTO VnInfoTags VALUES(@PK_Id, @VnId, @TagId, @TagName, @Score, @Spoiler);";
                                    cmd = new SQLiteCommand(sql, connection, transaction);
                                    cmd.Parameters.AddWithValue("@PK_Id", null);
                                    cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                                    cmd.Parameters.AddWithValue("@TagId", CheckForDbNull(tag.Id));
                                    cmd.Parameters.AddWithValue("@TagName", CheckForDbNull(tagMatches.ElementAt(count).Name));
                                    cmd.Parameters.AddWithValue("@Score", CheckForDbNull(tag.Score));
                                    cmd.Parameters.AddWithValue("@Spoiler", CheckForDbNull(tag.SpoilerLevel.ToString() ?? null));
                                    cmd.ExecuteNonQuery();
                                    count++;
                                }

                            }
                        }

                        #endregion

                        #region TagData

                        if (tagMatches != null)
                        {
                            foreach (Tag tag in tagMatches)
                            {
                                sql = "INSERT OR REPLACE INTO VnTagData VALUES(@PK_Id, @TagId, @Name, @Description, @Meta, @Vns, @Cat, @Aliases, @Parents);";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@TagId", CheckForDbNull(tag.Id));
                                cmd.Parameters.AddWithValue("@Name", CheckForDbNull(tag.Name));
                                cmd.Parameters.AddWithValue("@Description", CheckForDbNull(tag.Description));
                                cmd.Parameters.AddWithValue("@Meta", CheckForDbNull(tag.IsMeta.ToString()));
                                cmd.Parameters.AddWithValue("@Vns", CheckForDbNull(tag.VisualNovels));
                                cmd.Parameters.AddWithValue("@Cat", CheckForDbNull(tag.TagCategory.ToString()));
                                cmd.Parameters.AddWithValue("@Aliases", CheckForDbNull(ConvertToCsv(tag.Aliases)));
                                string parents = string.Join(",", tag.Parents);
                                cmd.Parameters.AddWithValue("@Parents", CheckForDbNull(parents));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region VnScreens

                        if (visualNovel.Screenshots.Count > 0)
                        {
                            foreach (ScreenshotMetadata screenshot in visualNovel.Screenshots)
                            {
                                sql =
                                    "INSERT OR REPLACE INTO VnInfoScreens VALUES(@PK_Id, @VnId, @ImageUrl, @ReleaseId, @Nsfw, @Height, @Width)";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                                cmd.Parameters.AddWithValue("@ImageUrl", CheckForDbNull(screenshot.Url));
                                cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(screenshot.ReleaseId));
                                cmd.Parameters.AddWithValue("@Nsfw", CheckForDbNull(screenshot.IsNsfw.ToString()));
                                cmd.Parameters.AddWithValue("@Height", CheckForDbNull(screenshot.Height));
                                cmd.Parameters.AddWithValue("@Width", CheckForDbNull(screenshot.Width));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        #endregion

                        #region VnInfoRelations

                        if (visualNovel.Relations.Count > 0)
                        {
                            foreach (VisualNovelRelation relation in visualNovel.Relations)
                            {
                                sql =
                                    "INSERT OR REPLACE INTO VnInfoRelations VALUES(@PK_Id, @VnId, @RelationId, @Relation, @Title, @Original, @Official)";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                                cmd.Parameters.AddWithValue("@RelationId", CheckForDbNull(relation.Id));
                                cmd.Parameters.AddWithValue("@Relation", CheckForDbNull(relation.Type.ToString()));
                                cmd.Parameters.AddWithValue("@Title", CheckForDbNull(relation.Title));
                                cmd.Parameters.AddWithValue("@Original", CheckForDbNull(relation.Original));
                                cmd.Parameters.AddWithValue("@Official",
                                    CheckForDbNull(relation.Official ? "Yes" : "No"));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        #endregion

                        #region VnInfoStaff

                        if (visualNovel.Staff.Count> 0)
                        {
                            foreach (StaffMetadata staff in visualNovel.Staff)
                            {
                                sql =
                                    "INSERT OR REPLACE INTO VnInfoStaff VALUES(@PK_Id, @VnId, @StaffId, @AliasId, @Name, @Original, @Role, @Note)";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                                cmd.Parameters.AddWithValue("@StaffId", CheckForDbNull(staff.StaffId));
                                cmd.Parameters.AddWithValue("@AliasId", CheckForDbNull(staff.AliasId));
                                cmd.Parameters.AddWithValue("@Name", CheckForDbNull(staff.Name));
                                cmd.Parameters.AddWithValue("@Original", CheckForDbNull(staff.Kanji));
                                cmd.Parameters.AddWithValue("@Role", CheckForDbNull(staff.Role));
                                cmd.Parameters.AddWithValue("@Note", CheckForDbNull(staff.Note));
                                cmd.ExecuteNonQuery();
                            }
                        }


                        #endregion
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region VnRelease
                    foreach (Release release in releases)
                    {
                        #region VnRelease

                        sql = "INSERT OR REPLACE INTO VnRelease VALUES(@PK_Id, @VnId, @ReleaseId, @Title, @Original, @Released, @ReleaseType, @Patch, @Freeware, @Doujin, @Languages," +
                              "@Website, @Notes, @MinAge, @Gtin, @Catalog, @Platforms)";
                        cmd = new SQLiteCommand(sql, connection, transaction);
                        cmd.Parameters.AddWithValue("@PK_Id", null);
                        cmd.Parameters.AddWithValue("@VnId", CheckForDbNull(Convert.ToInt32(visualNovels.Items[0].Id)));
                        cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(release.Id));
                        cmd.Parameters.AddWithValue("@Title", CheckForDbNull(release.Name));
                        cmd.Parameters.AddWithValue("@Original", CheckForDbNull(release.OriginalName));
                        cmd.Parameters.AddWithValue("@Released", CheckForDbNull(release.Released?.ToString() ?? null));
                        cmd.Parameters.AddWithValue("@ReleaseType", CheckForDbNull(release.Type.ToString() ?? null));
                        cmd.Parameters.AddWithValue("@Patch", CheckForDbNull(release.IsPatch.ToString()));
                        cmd.Parameters.AddWithValue("@Freeware", CheckForDbNull(release.IsFreeware.ToString()));
                        cmd.Parameters.AddWithValue("@Doujin", CheckForDbNull(release.IsDoujin.ToString()));
                        cmd.Parameters.AddWithValue("@Languages", CheckForDbNull(ConvertToCsv(release.Languages)));
                        cmd.Parameters.AddWithValue("@Website", CheckForDbNull(release.Website));
                        cmd.Parameters.AddWithValue("@Notes", CheckForDbNull(release.Notes));
                        cmd.Parameters.AddWithValue("@MinAge", CheckForDbNull(release.MinimumAge));
                        cmd.Parameters.AddWithValue("@Gtin", CheckForDbNull(release.Gtin));
                        cmd.Parameters.AddWithValue("@Catalog", CheckForDbNull(release.Catalog));
                        cmd.Parameters.AddWithValue("@Platforms", CheckForDbNull(ConvertToCsv(release.Platforms)));

                        cmd.ExecuteNonQuery();

                        #endregion

                        #region VnReleaseMedia

                        if (release.Media.Count <= 0) continue;
                        foreach (Media media in release.Media)
                        {
                            sql = "INSERT OR REPLACE INTO VnReleaseMedia VALUES(@PK_Id, @ReleaseId, @Medium, @Quantity)";
                            cmd = new SQLiteCommand(sql, connection, transaction);
                            cmd.Parameters.AddWithValue("@PK_Id", null);
                            cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(release.Id));
                            cmd.Parameters.AddWithValue("@Medium", CheckForDbNull(media.Medium));
                            cmd.Parameters.AddWithValue("@Quantity", CheckForDbNull(media.Quantity));
                            cmd.ExecuteNonQuery();
                        }

                        #endregion

                        #region VnReleaseVn

                        if (release.VisualNovels.Count <= 0) continue;
                        foreach (VisualNovelMetadata vn in release.VisualNovels)
                        {
                            sql = "INSERT OR REPLACE INTO VnReleaseVn VALUES(@PK_Id, @ReleaseId, @VnId, @Name, @Original)";
                            cmd = new SQLiteCommand(sql, connection, transaction);
                            cmd.Parameters.AddWithValue("@PK_Id", null);
                            cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(release.Id));
                            cmd.Parameters.AddWithValue("@VnId", CheckForDbNull(Convert.ToInt32(vn.Id)));
                            cmd.Parameters.AddWithValue("@Name", CheckForDbNull(vn.Name));
                            cmd.Parameters.AddWithValue("@Original", CheckForDbNull(vn.OriginalName));
                            cmd.ExecuteNonQuery();
                        }


                        #endregion

                        #region VnReleaseProducers

                        if (release.Producers.Count <= 0) continue;
                        foreach (ProducerRelease producer in release.Producers)
                        {
                            sql = "INSERT OR REPLACE INTO VnReleaseProducers VALUES(@PK_Id, @ReleaseId, @ProducerId, @Developer, @Publisher, @Name, @Original, @ProducerType)";
                            cmd = new SQLiteCommand(sql, connection, transaction);
                            cmd.Parameters.AddWithValue("@PK_Id", null);
                            cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(release.Id));
                            cmd.Parameters.AddWithValue("@ProducerId", CheckForDbNull(producer.Id));
                            cmd.Parameters.AddWithValue("@Developer", CheckForDbNull(producer.IsDeveloper.ToString()));
                            cmd.Parameters.AddWithValue("@Publisher", CheckForDbNull(producer.IsPublisher.ToString()));
                            cmd.Parameters.AddWithValue("@Name", CheckForDbNull(producer.Name));
                            cmd.Parameters.AddWithValue("@Original", CheckForDbNull(producer.OriginalName));
                            cmd.Parameters.AddWithValue("@ProducerType", CheckForDbNull(producer.ProducerType));
                            cmd.ExecuteNonQuery();
                        }

                        #endregion
                        
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region Character

                    IEnumerable<Trait> traitDump = await VndbUtils.GetTraitsDumpAsync();
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;

                    foreach (Character character in characters)
                    {
                        #region VnCharacter

                        sql = "INSERT OR REPLACE INTO VnCharacter VALUES(@PK_Id, @VnId, @CharacterId, @Name, @Original, @Gender, @BloodType, @Birthday, @Aliases, @Description," +
                              "@ImageLink, @Bust, @Waist, @Hip, @Height, @Weight)";
                        cmd = new SQLiteCommand(sql, connection, transaction);
                        cmd.Parameters.AddWithValue("@PK_Id", null);
                        cmd.Parameters.AddWithValue("@VnId",
                            CheckForDbNull(Convert.ToInt32(visualNovels.Items[0].Id)));
                        cmd.Parameters.AddWithValue("@CharacterId", CheckForDbNull(character.Id));
                        cmd.Parameters.AddWithValue("@Name", CheckForDbNull(character.Name));
                        cmd.Parameters.AddWithValue("@Original", CheckForDbNull(character.OriginalName));
                        cmd.Parameters.AddWithValue("@Gender", CheckForDbNull(character.Gender?.ToString() ?? null));
                        cmd.Parameters.AddWithValue("@BloodType", CheckForDbNull(character.BloodType?.ToString() ?? null));
                        cmd.Parameters.AddWithValue("@Birthday", CheckForDbNull(ConvertBirthday(character.Birthday)));                        
                        cmd.Parameters.AddWithValue("@Aliases", CheckForDbNull(ConvertToCsv(character.Aliases)));
                        cmd.Parameters.AddWithValue("@Description", CheckForDbNull(character.Description));
                        cmd.Parameters.AddWithValue("@ImageLink", CheckForDbNull(character.Image));
                        cmd.Parameters.AddWithValue("@Bust", CheckForDbNull(character.Bust));
                        cmd.Parameters.AddWithValue("@Waist", CheckForDbNull(character.Waist));
                        cmd.Parameters.AddWithValue("@Hip", CheckForDbNull(character.Hip));
                        cmd.Parameters.AddWithValue("@Height", CheckForDbNull(character.Height));
                        cmd.Parameters.AddWithValue("@Weight", CheckForDbNull(character.Weight));
                        cmd.ExecuteNonQuery();

                        #endregion

                        #region VnCharacterTraits

                        if (character.Traits.Count < 1) break;
                        IEnumerable<Trait> traitMatches = GetDetailsFromTraitDump(traitDump, character.Traits);
                        int count = 0;
                        foreach (TraitMetadata trait in character.Traits)
                        {
                            if (traitMatches.Any(c => c.Id == trait.Id))//prevents crashes with awaiting traits
                            {
                                sql = "INSERT OR REPLACE INTO VnCharacterTraits VALUES(@PK_Id, @CharacterId, @TraitId, @TraitName, @SpoilerLevel)";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@CharacterId", CheckForDbNull(character.Id));
                                cmd.Parameters.AddWithValue("@TraitId", CheckForDbNull(trait.Id));
                                cmd.Parameters.AddWithValue("@TraitName", CheckForDbNull(traitMatches.ElementAt(count).Name));
                                cmd.Parameters.AddWithValue("@SpoilerLevel", CheckForDbNull(trait.SpoilerLevel.ToString() ?? null));
                                cmd.ExecuteNonQuery();
                                count++;
                            }

                        }
                        #endregion

                        #region TraitMatches
                        if (traitMatches != null)
                        {
                            foreach (Trait trait in traitMatches)
                            {
                                sql = "INSERT OR REPLACE INTO VnTraitData VALUES(@PK_Id, @TraitId, @Name, @Description, @Meta, @Chars, @Aliases, @Parents);";
                                cmd = new SQLiteCommand(sql, connection, transaction);
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@TraitId", CheckForDbNull(trait.Id));
                                cmd.Parameters.AddWithValue("@Name", CheckForDbNull(trait.Name));
                                cmd.Parameters.AddWithValue("@Description", CheckForDbNull(trait.Description));
                                cmd.Parameters.AddWithValue("@Meta", CheckForDbNull(trait.IsMeta.ToString()));
                                cmd.Parameters.AddWithValue("@Chars", CheckForDbNull(trait.Characters));
                                cmd.Parameters.AddWithValue("@Aliases", CheckForDbNull(ConvertToCsv(trait.Aliases)));
                                string parents = string.Join(",", trait.Parents);
                                cmd.Parameters.AddWithValue("@Parents", CheckForDbNull(parents));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region VnCharacterVns

                        foreach (VndbSharp.Models.Character.VisualNovelMetadata vn in character.VisualNovels)
                        {
                            sql = "INSERT OR REPLACE INTO VnCharacterVns VALUES(@PK_Id, @CharacterId, @VnId, @ReleaseId, @SpoilerLevel, @Role)";
                            cmd = new SQLiteCommand(sql, connection, transaction);
                            cmd.Parameters.AddWithValue("@PK_Id", null);
                            cmd.Parameters.AddWithValue("@CharacterId", CheckForDbNull(character.Id));
                            cmd.Parameters.AddWithValue("@VnId", CheckForDbNull(vn.Id));
                            cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(vn.ReleaseId));
                            cmd.Parameters.AddWithValue("@SpoilerLevel", CheckForDbNull(vn.SpoilerLevel.ToString() ?? null));
                            cmd.Parameters.AddWithValue("@Role", CheckForDbNull(vn.Role.ToString() ?? null));
                            cmd.ExecuteNonQuery();
                        }
                        #endregion
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    #region VnUserData

                    sql = "INSERT OR REPLACE INTO VnUserData VALUES(@PK_Id, @VnId, @ExePath, @IconPath, @LastPlayed, @PlayTime)";
                    cmd = new SQLiteCommand(sql, connection, transaction);
                    cmd.Parameters.AddWithValue("@PK_Id", null);
                    cmd.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                    cmd.Parameters.AddWithValue("@ExePath", _exepath);
                    cmd.Parameters.AddWithValue("@IconPath", CheckForDbNull(_iconpath));
                    cmd.Parameters.AddWithValue("@LastPlayed", "");
                    cmd.Parameters.AddWithValue("@PlayTime", "0,0,0,0");

                    cmd.ExecuteNonQuery();

                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + ProgressIncrement;
                    #endregion

                    transaction.Commit();

                    cmd.Dispose();


                }
                catch (SQLiteException ex)
                {
                    DebugLogging.WriteDebugLog(ex);
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                    
                }
                connection.Close();
            }            
        }

        async Task AddDbTest(VndbResponse<VisualNovel> visualNovels, Collection<Character> characters, Collection<Release> releases)
        {
            
            using (var db = new DatabaseContext("name=Database"))
            {
                #region VnInfo
                var vninfo = db.Set<VnInfo>();
                var vninfoanime = db.Set<EntityFramework.Entity.VnInfo.VnInfoAnime>();
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
                        VnId = Convert.ToInt32(visualNovel.Id),
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
                        vninfoanime.Add(new EntityFramework.Entity.VnInfo.VnInfoAnime
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
                                    VnId = Convert.ToInt32(visualNovel.Id),
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
                                VnId = Convert.ToInt32(visualNovel.Id),
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
                                VnId = Convert.ToInt32(visualNovel.Id),
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
                            VnId = Convert.ToInt32(vn.Id),
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
                    foreach (VisualNovelMetadata vn in release.VisualNovels)
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


                db.SaveChanges();
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

        private bool IsValidResponse<T>(VndbResponse<T> response, Vndb client)
        {
            if (response == null)
            {
                HandleError(client.GetLastError());
                return false;
            }
            if (response.Count == 0)
            {
                return false;
            }
            return true;
        }



        //this forces entries that are null to work in the database
        public object CheckForDbNull(object value)
        {
            return value ?? DBNull.Value;
        }

        private static void HandleError(IVndbError error)
        {
            if (error is MissingError missing)
            {
                Console.WriteLine($"A Missing Error occured, the field \"{missing.Field}\" was missing.");
            }
            else if (error is BadArgumentError badArg)
            {
                Console.WriteLine($"A BadArgument Error occured, the field \"{badArg.Field}\" is invalid.");
            }
            else if (error is ThrottledError throttled)
            {
                double minSeconds = (DateTime.Now - throttled.MinimumWait).TotalSeconds; // Not sure if this is correct
                double fullSeconds = (DateTime.Now - throttled.FullWait).TotalSeconds; // Not sure if this is correct
                Console.WriteLine(
                    $"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
                    $"and preferably \"{fullSeconds}\" before issuing commands.");
            }
            else if (error is GetInfoError getInfo)
            {
                Console.WriteLine($"A GetInfo Error occured, the flag \"{getInfo.Flag}\" is not valid on the issued command.");
            }
            else if (error is InvalidFilterError invalidFilter)
            {
                Console.WriteLine(
                    $"A InvalidFilter Error occured, the filter combination of \"{invalidFilter.Field}\", " +
                    $"\"{invalidFilter.Operator}\", \"{invalidFilter.Value}\" is not a valid combination.");
            }
            else
            {
                Console.WriteLine($"A {error.Type} Error occured.");
            }
            Console.WriteLine($"Message: {error.Message}");
        }
    }
}
