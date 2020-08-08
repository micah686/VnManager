using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using LiteDB;
using VndbSharp.Models.Common;
using VnManager.Converters;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.Vndb.Main;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
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

        private BitmapSource _testImg;

        public BitmapSource TestImg
        {
            get
            {
                _testImg = ImageHelper.CreateEmptyBitmapImage();
                return _testImg;
            }
            set
            {
                _testImg = value;
                SetAndNotify(ref _testImg, value);
            }
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
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null || cred.UserName.Length < 1) return;
            using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
            {
                var dbUserData = db.GetCollection<UserDataGames>("UserData_Games");
                var entry = new UserDataGames();
                entry.SourceType = AddGameSourceType.Vndb;
                entry.ExeType = ExeTypeEnum.Launcher;
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
            //new Helpers.Secure().SetSecret("VndbPass", "samplepassword12345!@#");
            //var foo = new Helpers.Secure().ReadSecret("VndbPass");
            //Secure.FileEncrypt("test.txt");
            //Secure.FileEncrypt("aaa.jpg");
            //Secure.FileDecrypt("aaa.jpg.aes");
        }

        public void TestStrings()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja-JP");
            var value = App.ResMan.GetString("AddGameCollectionTitle");
        }

        public void TestPasswordEntry()
        {
            var vmTestPassword = _container.Get<SetEnterPasswordViewModel>();

            _windowManager.ShowWindow(vmTestPassword);
            App.StatusBar.IsProgressBarVisible = true;
            App.StatusBar.IsProgressBarInfinite = false;
        }

        
        public async Task TestStatusBar()
        {
            
            
            //var client = new WebClient(); 
            //var stream = new MemoryStream(await client.DownloadDataTaskAsync(new Uri("https://s2.vndb.org/sf/33/233.jpg")));
            //var img = SaveVnDataToDb.GetThumbnailImage(stream);
            //var sv = new SaveVnDataToDb().DownloadScreenshots(4857);
            //new Secure().TestPassHash();
            var save = new SaveVnDataToDb();
            await save.DownloadScreenshots(15538);
        }

        public void TestEncryption()
        {
            //Secure.EncFile("sample.jpg");

            //Secure.DecFile("sample.jpg");
            //_windowManager.ShowMessageBox("THis is a test message", "Title Here", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            var ts = new  TimeSpan(7,15,1,0);
            Helpers.TimeDateChanger.GetHumanTime(ts);
        }

        public void ExportImport()
        {
            var vm = _container.Get<ImportExportDataViewModel>();
            _windowManager.ShowDialog(vm);
        }

        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        internal static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
