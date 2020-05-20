using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using VnManager.MetadataProviders.Vndb;

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
            var foo = new GetVndbData();
            foo.GetData(92);
        }

        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
