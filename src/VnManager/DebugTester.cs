using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using VnManager.Utilities;
using VnManager.Converters;

namespace VnManager
{
    [ExcludeFromCodeCoverage]
    public class DebugTester
    {
        public void Tester()
        {
            
            App.Logger.Error("test");
            //CauseException();
        }
        



        private void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }
        
        [DllImport("kernel32.dll")]
        public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
