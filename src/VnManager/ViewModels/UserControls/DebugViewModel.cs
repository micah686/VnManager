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
using System.Threading;
using System.Threading.Tasks;
using StyletIoC;
using VndbSharp;
using VndbSharp.Models.Dumps;
using VnManager.Helpers;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.Windows;


namespace VnManager.ViewModels.UserControls
{
    [ExcludeFromCodeCoverage]
    public class DebugViewModel: Screen
    {
        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;

        public DebugViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
        }



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
            UserSettingsHelper.CreateDefaultConfig();
            UserSettingsHelper.ValidateConfigFile();
            UserSettingsHelper.ReadUserSettings();

        }


        public void AddUserData()
        {
            using (var db = new LiteDatabase(App.GetDatabaseString()))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games");
                var entry = new UserDataGames();
                entry.SourceType = AddGameMainViewModel.AddGameSourceType.Vndb;
                entry.ExeType = AddGameMainViewModel.ExeTypeEnum.Launcher;
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
            //new Helpers.EncryptedStore().FileEncrypt("test.txt", "FileEnc");
            new Helpers.Secure().SetSecret("VndbPass", "samplepassword12345!@#");
            var foo = new Helpers.Secure().ReadSecret("VndbPass");
        }

        public void TestStrings()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja-JP");
            ResourceManager rm = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());
            var value = rm.GetString("AddGameCollectionTitle");
        }

        public void TestPasswordEntry()
        {
            var vmTestPassword = _container.Get<SetEnterPasswordViewModel>();

            _windowManager.ShowWindow(vmTestPassword);
            App.StatusBar.IsProgressBarVisible = true;
            App.StatusBar.IsProgressBarInfinite = false;
        }

        private double currentVal = 5.0;
        public void TestStatusBar()
        {
            
            
            //App.StatusBar.ProgressBarValue = 0;
            //App.StatusBar.IsWorking = true;
            //App.StatusBar.InfoText = "debug test";
            //App.StatusBar.IsDatabaseProcessing = true;
            currentVal += 10;
            App.StatusBar.ProgressBarValue = currentVal;
        }


        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        internal static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
