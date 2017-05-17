using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VndbSharp;
using VndbSharp.Interfaces;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Errors;
using VndbSharp.Models.Producer;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;
using VisualNovelMetadata = VndbSharp.Models.Release.VisualNovelMetadata;

namespace VisualNovelManagerv2.CustomClasses.Database
{
    public class AddToDatabase
    {
        private int _vnid;
        private uint _uvnid;
        private string exepath;

        public async void GetId(int id, string exe)
        {
            _vnid = id;
            _uvnid = Convert.ToUInt32(id);
            exepath = exe;
            await GetData();
        }

        async Task GetData()
        {            
            DatabaseStats stats = null;
            List<object>[] data = new List<object>[5];
            

            using (Vndb client = new Vndb(true).WithClientDetails(Globals.ClientInfo[0], Globals.ClientInfo[1]))
            {
                //stats = await client.GetDatabaseStatsAsync();
                VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(_uvnid), VndbFlags.FullVisualNovel);
                VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullRelease);
                //VndbResponse<Producer> producers = await client.GetProducerAsync(VndbFilters.Id.Equals(9), VndbFlags.FullProducer);
                VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullCharacter);
                //AddDataToDbVn(visualNovels);
                //AddDataToDbUserData(visualNovels);
                AddDataToDbReleases(visualNovels, releases, characters);
                //Console.WriteLine();
                Thread.Sleep(0);
            }

        }

        void AddDataToDbVn(VndbResponse<VisualNovel> visualNovels)
        {
            lock (Globals.WriteLock)
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteTransaction trans= connection.BeginTransaction())
                    {
                        using (SQLiteCommand query = new SQLiteCommand("", connection, trans))
                        {
                            query.CommandText =
                                "INSERT OR REPLACE INTO VnInfo VALUES(@PK_Id, @VnId, @Title, @Original, @Released, @Languages, @OriginalLanguage, @Platforms, @Aliases, @Length," +
                                " @Description, @ImageLink, @ImageNsfw, @Popularity, @Rating);";
                            query.Parameters.AddWithValue("@PK_Id", null);
                            query.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                            query.Parameters.AddWithValue("@Title", visualNovels.Items[0].Name);
                            query.Parameters.AddWithValue("@Original", visualNovels.Items[0].OriginalName);
                            query.Parameters.AddWithValue("@Released", visualNovels.Items[0].Released.ToString());
                            string vnlang = string.Join(",", visualNovels.Items[0].Languages);
                            query.Parameters.AddWithValue("@Languages", vnlang);
                            string origvnlang = string.Join(",", visualNovels.Items[0].OriginalLanguages);
                            query.Parameters.AddWithValue("@OriginalLanguage", origvnlang);
                            string vnplat = string.Join(",", visualNovels.Items[0].Platforms);
                            query.Parameters.AddWithValue("@Platforms", vnplat);
                            string vnalias = string.Join(",", visualNovels.Items[0].Aliases);
                            query.Parameters.AddWithValue("@Aliases", vnalias);
                            query.Parameters.AddWithValue("@Length", visualNovels.Items[0].Length);
                            query.Parameters.AddWithValue("@Description", visualNovels.Items[0].Description);
                            query.Parameters.AddWithValue("@ImageLink", visualNovels.Items[0].Image);
                            query.Parameters.AddWithValue("@ImageNsfw", visualNovels.Items[0].IsImageNsfw.ToString());
                            query.Parameters.AddWithValue("@Popularity", visualNovels.Items[0].Popularity);
                            query.Parameters.AddWithValue("@Rating", visualNovels.Items[0].Rating);

                            query.ExecuteNonQuery();


                            //VnInfoLinks
                            //query.CommandText =
                            //    "INSERT OR REPLACE INTO VnInfoLinks VALUES(@PK_Id, @VnId, @Wikipedia, @Encubed, @Renai)";
                            //query.Parameters.AddWithValue("@PK_Id", null);
                            //query.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                            //query.Parameters.AddWithValue("@", visualNovels.Items[0].VisualNovelLinks.Wikipedia);


                            //VnAnime
                            if (visualNovels.Items[0].Anime.Length >= 1)
                            {
                                foreach (AnimeMetadata anime in visualNovels.Items[0].Anime)
                                {
                                    query.CommandText =
                                        "INSERT OR REPLACE INTO VnAnime VALUES(@PK_Id, @VnId, @AniDbId, @AnnId, @AniNfoId, @TitleEng, @TitleJpn, @Year, @AnimeType);";
                                    query.Parameters.AddWithValue("@PK_Id", null);
                                    query.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                                    query.Parameters.AddWithValue("@AniDbId", CheckForDbNull(anime.AniDbId));
                                    query.Parameters.AddWithValue("@AnnId", CheckForDbNull(anime.AnimeNewsNetworkId));
                                    query.Parameters.AddWithValue("@AniNfoId", CheckForDbNull(anime.AnimeNfoId));
                                    query.Parameters.AddWithValue("@TitleEng", CheckForDbNull(anime.RomajiTitle));
                                    query.Parameters.AddWithValue("@TitleJpn", CheckForDbNull(anime.KanjiTitle));
                                    query.Parameters.AddWithValue("@Year", CheckForDbNull(anime.AiringYear.ToString()));
                                    query.Parameters.AddWithValue("@AnimeType", CheckForDbNull(anime.Type));
                                    query.ExecuteNonQuery();
                                }
                            }


                            //VnTags
                            foreach (TagMetadata tag in visualNovels.Items[0].Tags)
                            {
                                query.CommandText =
                                    "INSERT OR REPLACE INTO VnTags VALUES(@PK_Id, @VnId, @TagId, @TagName, @Score, @Spoiler);";
                                query.Parameters.AddWithValue("@PK_Id", null);
                                query.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                                query.Parameters.AddWithValue("@TagId", CheckForDbNull(tag.Id));
                                query.Parameters.AddWithValue("@TagName", CheckForDbNull(null));
                                query.Parameters.AddWithValue("@Score", CheckForDbNull(tag.Score));
                                query.Parameters.AddWithValue("@Spoiler", CheckForDbNull(tag.SpoilerLevel.ToString()));
                                query.ExecuteNonQuery();
                                
                            }


                        }
                        trans.Commit();
                    }
                    
                }
            }
        }

        void AddDataToDbUserData(VndbResponse<VisualNovel> visualNovels)
        {
            lock (Globals.WriteLock)
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand query = new SQLiteCommand(connection))
                    {
                        object[] querytmp = { query };
                        query.CommandText =
                            "INSERT OR REPLACE INTO VnUserData VALUES(@PK_Id, @VnId, @ExePath, @IconPath, @LastPlayed, @SecondsPlayed, @Categories)";
                        query.Parameters.AddWithValue("@PK_Id", null);
                        query.Parameters.AddWithValue("@VnId", Convert.ToInt32(visualNovels.Items[0].Id));
                        query.Parameters.AddWithValue("@ExePath", exepath);
                        query.Parameters.AddWithValue("@IconPath", null);
                        query.Parameters.AddWithValue("@LastPlayed", null);
                        query.Parameters.AddWithValue("@SecondsPlayed", null);
                        query.Parameters.AddWithValue("@Categories", null);



                        query.ExecuteNonQuery();
                        Console.WriteLine("done");
                    }
                }
            }
        }

        void AddDataToDbReleases(VndbResponse<VisualNovel> visualNovels, VndbResponse<Release> releases, VndbResponse<Character> characters)
        {
            lock (Globals.WriteLock)
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            #region VnRelease
                            foreach (Release release in releases)
                            {
                                #region VnRelease
                                cmd.CommandText =
                                    "INSERT OR REPLACE INTO VnRelease VALUES(@PK_Id, @VnId, @ReleaseId, @Title, @Original, @Released, @ReleaseType, @Patch, @Freeware, @Doujin, @Languages," +
                                    "@Website, @Notes, @MinAge, @Gtin, @Catalog, @Platforms)";
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@VnId", CheckForDbNull(Convert.ToInt32(visualNovels.Items[0].Id)));
                                cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(release.Id));
                                cmd.Parameters.AddWithValue("@Title", CheckForDbNull(release.Name));
                                cmd.Parameters.AddWithValue("@Original", CheckForDbNull(release.OriginalName));
                                cmd.Parameters.AddWithValue("@Released", CheckForDbNull(release.Released.ToString()));
                                cmd.Parameters.AddWithValue("@ReleaseType", CheckForDbNull(release.Type.ToString()));
                                cmd.Parameters.AddWithValue("@Patch", CheckForDbNull(release.IsPatch.ToString()));
                                cmd.Parameters.AddWithValue("@Freeware", CheckForDbNull(release.IsFreeware.ToString()));
                                cmd.Parameters.AddWithValue("@Doujin", CheckForDbNull(release.IsDoujin.ToString()));
                                string rellang = string.Join(",", release.Languages);
                                cmd.Parameters.AddWithValue("@Languages", CheckForDbNull(rellang));
                                cmd.Parameters.AddWithValue("@Website", CheckForDbNull(release.Website));
                                cmd.Parameters.AddWithValue("@Notes", CheckForDbNull(release.Notes));
                                cmd.Parameters.AddWithValue("@MinAge", CheckForDbNull(release.MinimumAge));
                                cmd.Parameters.AddWithValue("@Gtin", CheckForDbNull(release.Gtin));
                                cmd.Parameters.AddWithValue("@Catalog", CheckForDbNull(release.Catalog));
                                string relplats = string.Join(",", release.Platforms);
                                cmd.Parameters.AddWithValue("@Platforms", CheckForDbNull(relplats));
                                cmd.ExecuteNonQuery();
                                #endregion

                                #region VnReleaseMedia
                                if (release.Media.Count <= 0) continue;
                                foreach (Media media in release.Media)
                                {
                                    cmd.CommandText =
                                        "INSERT OR REPLACE INTO VnReleaseMedia VALUES(@PK_Id, @ReleaseId, @Medium, @Quantity)";
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
                                    cmd.CommandText =
                                        "INSERT OR REPLACE INTO VnReleaseVn VALUES(@PK_Id, @ReleaseId, @VnId, @Name, @Original)";
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
                                    cmd.CommandText = "INSERT OR REPLACE INTO VnReleaseProducers VALUES(@PK_Id, @ReleaseId, @ProducerId, @Developer, @Publisher, @Name, @Original, @ProducerType)";
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

                            #endregion

                            
                            foreach (Character character in characters)
                            {
                                #region VnCharacter
                                cmd.CommandText = "INSERT OR REPLACE INTO VnCharacter VALUES(@PK_Id, @VnId, @CharacterId, @Name, @Original, @Gender, @BloodType, @Birthday, @Aliases, @Description," +
                                                  "@ImageLink, @Bust, @Waist, @Hip, @Height, @Weight)";
                                cmd.Parameters.AddWithValue("@PK_Id", null);
                                cmd.Parameters.AddWithValue("@VnId", CheckForDbNull(Convert.ToInt32(visualNovels.Items[0].Id)));
                                cmd.Parameters.AddWithValue("@CharacterId", CheckForDbNull(character.Id));
                                cmd.Parameters.AddWithValue("@Name", CheckForDbNull(character.Name));
                                cmd.Parameters.AddWithValue("@Original", CheckForDbNull(character.OriginalName));
                                cmd.Parameters.AddWithValue("@Gender", CheckForDbNull(character.Gender.ToString()));
                                cmd.Parameters.AddWithValue("@BloodType", CheckForDbNull(character.BloodType.ToString()));
                                cmd.Parameters.AddWithValue("@Birthday", CheckForDbNull(character.Birthday.ToString()));
                                string charalias = string.Join(",", character.Aliases);
                                cmd.Parameters.AddWithValue("@Aliases", CheckForDbNull(charalias));
                                cmd.Parameters.AddWithValue("@Description", CheckForDbNull(character.Description));
                                cmd.Parameters.AddWithValue("@ImageLink", CheckForDbNull(character.Image));
                                cmd.Parameters.AddWithValue("@Bust", CheckForDbNull(Convert.ToInt32(character.Bust)));
                                cmd.Parameters.AddWithValue("@Waist", CheckForDbNull(Convert.ToInt32(character.Waist)));
                                cmd.Parameters.AddWithValue("@Hip", CheckForDbNull(Convert.ToInt32(character.Hip)));
                                cmd.Parameters.AddWithValue("@Height", CheckForDbNull(Convert.ToInt32(character.Height)));
                                cmd.Parameters.AddWithValue("@Weight", CheckForDbNull(Convert.ToInt32(character.Weight)));
                                cmd.ExecuteNonQuery();
                                #endregion

                                #region VnCharacterTraits
                                if (character.Traits.Count >0) continue;
                                foreach (TraitMetadata trait in character.Traits)
                                {
                                    cmd.CommandText =
                                        "INSERT OR REPLACE INTO VnCharacterTraits VALUES(@PK_Id, @CharacterId, @SpoilerLevel)";
                                    cmd.Parameters.AddWithValue("@PK_Id", null);
                                    cmd.Parameters.AddWithValue("@CharacterId", CheckForDbNull(trait.Id));
                                    cmd.Parameters.AddWithValue("@SpoilerLevel", CheckForDbNull(trait.SpoilerLevel.ToString()));
                                    cmd.ExecuteNonQuery();
                                }

                                #endregion

                                #region VnCharacterVns
                                if (character.VisualNovels.Count > 0) continue;
                                foreach (VndbSharp.Models.Character.VisualNovelMetadata vn in character.VisualNovels)
                                {
                                    cmd.CommandText =
                                        "INSERT OR REPLACE INTO VnCharacterVns VALUES(@PK_Id, @CharacterId, @VnId, @ReleaseId, @SpoilerLevel, @Role)";
                                    cmd.Parameters.AddWithValue("@PK_Id", null);
                                    cmd.Parameters.AddWithValue("@CharacterId", CheckForDbNull(character.Id));
                                    cmd.Parameters.AddWithValue("@VnId", CheckForDbNull(vn.Id));
                                    cmd.Parameters.AddWithValue("@ReleaseId", CheckForDbNull(vn.ReleaseId));
                                    cmd.Parameters.AddWithValue("@SpoilerLevel", CheckForDbNull(vn.SpoilerLevel.ToString()));
                                    cmd.Parameters.AddWithValue("@Role", CheckForDbNull(vn.Role.ToString()));
                                    cmd.ExecuteNonQuery();
                                }


                                #endregion
                            }

                            



                        }
                        transaction.Commit();
                    }
                    connection.Close();
                }


                //using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
                //{
                //    connection.Open();
                //    using (SQLiteTransaction transaction = connection.BeginTransaction())
                //    {
                //        using (SQLiteCommand cmd = connection.CreateCommand())
                //        {

                //        }
                //    }
                //}
            }
        }



        //void AddDataToDb(VndbResponse<VisualNovel> visualNovels)
        //{
        //    lock (Globals.writeLock)
        //    {
        //        using (SQLiteConnection connection = new SQLiteConnection(Globals.connectionString))
        //        {
        //            connection.Open();
        //            using (SQLiteCommand query = new SQLiteCommand(connection))
        //            {
        //                query.CommandText =
        //                    "INSERT OR REPLACE INTO VnInfo VALUES(@PK_Id, @VnId, @Title, @Original, @Released, @Languages, @OriginalLanguage, @Platforms, @Aliases, @Length," +
        //                    " @Description, @ImageLink, @ImageNsfw, @Popularity, @Rating)";
        //                query.Parameters.AddWithValue("@PK_Id",null);
        //                query.Parameters.AddWithValue("@VnId",Convert.ToInt32(visualNovels.Items[0].Id));
        //                query.Parameters.AddWithValue("@Title", visualNovels.Items[0].Name);
        //                query.Parameters.AddWithValue("@Original", visualNovels.Items[0].OriginalName);
        //                query.Parameters.AddWithValue("@Released", visualNovels.Items[0].Released.ToString());
        //                string vnlang = string.Join(",", visualNovels.Items[0].Languages);
        //                query.Parameters.AddWithValue("@Languages", vnlang);
        //                string origvnlang = string.Join(",", visualNovels.Items[0].OriginalLanguages);
        //                query.Parameters.AddWithValue("@OriginalLanguage", origvnlang);
        //                string vnplat = string.Join(",", visualNovels.Items[0].Platforms);
        //                query.Parameters.AddWithValue("@Platforms", vnplat);
        //                string vnalias = string.Join(",", visualNovels.Items[0].Aliases);
        //                query.Parameters.AddWithValue("@Aliases", vnalias);
        //                query.Parameters.AddWithValue("@Length", visualNovels.Items[0].Length);
        //                query.Parameters.AddWithValue("@Description", visualNovels.Items[0].Description);
        //                query.Parameters.AddWithValue("@ImageLink", visualNovels.Items[0].Image);
        //                query.Parameters.AddWithValue("@ImageNsfw", visualNovels.Items[0].IsImageNsfw.ToString());
        //                query.Parameters.AddWithValue("@Popularity", visualNovels.Items[0].Popularity);
        //                query.Parameters.AddWithValue("@Rating", visualNovels.Items[0].Rating);

        //                query.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //}


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

        private void HandleError(IVndbError error)
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
                var minSeconds = (DateTime.Now - throttled.MinimumWait).TotalSeconds; // Not sure if this is correct
                var fullSeconds = (DateTime.Now - throttled.FullWait).TotalSeconds; // Not sure if this is correct
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
