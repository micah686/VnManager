using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using LiteDB;
using VndbSharp.Models.Common;
using VnManager.Converters;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.Vndb.Main;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Resources;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models.Dumps;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.ViewModels.Windows;


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
            //foo.GetData(92);
            //DoThingAsync();
            //var foo2 = new SaveVnDataToDb();
            //foo2.GetAndSaveTraitDump();



        }


        public void AddUserData()
        {
            using (var db = new LiteDatabase(App.DatabasePath))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games");
                var entry = new UserDataGames();
                entry.SourceType = AddGameSourceTypes.Vndb;
                entry.ExeType = ExeTypesEnum.Launcher;
                entry.Id = Guid.NewGuid();
                entry.GameId = new Random().Next();
                entry.LastPlayed = DateTime.UtcNow;
                entry.PlayTime = TimeSpan.FromDays(3.5);
                entry.ExePath = @"C:\test.exe";
                entry.IconPath = @"C:\test.ico";
                entry.Arguments = "- quiet";
                dbUserData.Insert(entry);

            }
        }

        public void CreateSecure()
        {
            new Helpers.EncryptedStore().FileEncrypt("test.txt", "FileEnc");
            new Helpers.EncryptedStore().FileDecrypt("test.txt.aes", "test_dec.txt", "FileEnc");
        }

        public void TestStrings()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja-JP");
            ResourceManager rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            var value = rm.GetString("AddGameCollectionTitle");
        }

        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        internal static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
