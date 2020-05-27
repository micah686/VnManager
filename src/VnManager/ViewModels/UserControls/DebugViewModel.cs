using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models.Dumps;
using VnManager.Models.Db.Vndb.TagTrait;


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
            
            //var foo = new GetVndbData();
            //foo.GetData(92);
            DoThingAsync();




        }


        private async Task DoThingAsync()
        {

        }

        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
