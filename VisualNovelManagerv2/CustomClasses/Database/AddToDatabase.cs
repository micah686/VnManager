using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
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
            

            using (Vndb client = new Vndb(true).WithClientDetails("VNMv2", "0.1"))
            {
                stats = await client.GetDatabaseStatsAsync();
                VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(_uvnid), VndbFlags.FullVisualNovel);
                VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullRelease);
                VndbResponse<Producer> producers = await client.GetProducerAsync(VndbFilters.Id.Equals(9), VndbFlags.FullProducer);
                VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullCharacter);
                AddDataToDbVn(visualNovels);
                AddDataToDbUserData(visualNovels);
                Console.WriteLine();
            }

        }

        void AddDataToDbVn(VndbResponse<VisualNovel> visualNovels)
        {
            lock (Globals.writeLock)
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand query = new SQLiteCommand(connection))
                    {
                        object[] querytmp = {query };
                        query.CommandText =
                            "INSERT OR REPLACE INTO VnInfo VALUES(@PK_Id, @VnId, @Title, @Original, @Released, @Languages, @OriginalLanguage, @Platforms, @Aliases, @Length," +
                            " @Description, @ImageLink, @ImageNsfw, @Popularity, @Rating)";
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
                    }
                }
            }
        }

        void AddDataToDbUserData(VndbResponse<VisualNovel> visualNovels)
        {
            lock (Globals.writeLock)
            {
                using (SQLiteConnection connection = new SQLiteConnection(Globals.connectionString))
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

        void AddDataToDbReleases(VndbResponse<Release> releases)
        {
            
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
