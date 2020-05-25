using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using LiteDB;
using VndbSharp.Models.Common;
using VnManager.Converters;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.Vndb.Main;
using System.Linq;

namespace VnManager.ViewModels.UserControls
{
    [ExcludeFromCodeCoverage]
    public class DebugViewModel: Screen
    {
        public DebugViewModel() { }



        public void WriteLog()
        {
            App.Logger.Error("DebugTest");
        }


        public void TestVndbGet()
        {
            var bd = BirthdayConverter.ConvertBirthday(new SimpleDate() {Day = 30, Month = 12, Year = 2000});
            
            var foo = new GetVndbData();
            foo.GetData(92);


            //using (var db = new LiteDatabase(App.DatabasePath))
            //{
            //    var col = db.GetCollection<VnInfo>("entry_coll");
            //    var entry = new VnInfo
            //    {
            //        VnId = 99,
            //        Title = "SampleTitle",
            //        Original = "FirstString"
            //    };
            //    col.Insert(entry);

            //    var entryUpd = new VnInfo();
            //    var prev = col.Query().Where(x => x.VnId == (uint) 99).FirstOrDefault();
            //    if (prev != null)
            //    {
            //        entryUpd = prev;
            //    }

            //    entryUpd.Original = "Sample2";
                
            //    col.Upsert(entryUpd);

            //}
            return;
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var vnAnime = new List<VnInfoAnime>();
                var col = db.GetCollection<VnInfoAnime>("collection");
                var prev = col.Query().Where(x => x.VnId == 1).ToList();
                if (prev.Count > 0)
                {
                    var vnInfo = new VnInfoAnime();
                    if (prev.Any(x => x.AniDbId == 3))
                    {
                        vnInfo = prev.First(x => x.AniDbId == 3);
                    }
                }
                else
                {
                    
                }
                
                //var vnAnime = new List<VnInfoAnime>();
                //if (prev.Count > 0)
                //{
                //    vnAnime = prev;
                //}

                //for (int i = 0; i < 10; i++)
                //{
                //    if (i % 2 ==0)
                //    {
                //        vnAnime[i].AnimeType = "updated";
                //    }
                //    else
                //    {
                //        var entry = new VnInfoAnime()
                //        {
                //            VnId = 50,
                //            AnnId = i,
                //            AnimeType = "test"
                //        };
                //        vnAnime.Add(entry);
                //    }
                //}

                //col.Upsert(vnAnime);


                //var vnAnime = new List<VnInfoAnime>();
                //for (int i = 0; i < 10; i++)
                //{
                //    var entry = new VnInfoAnime()
                //    {
                //        VnId = 50,
                //        AnnId = i,
                //        AnimeType = "test"
                //    };
                //    vnAnime.Add(entry);
                //}
                //col.Upsert(vnAnime);
            }

        }

        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
