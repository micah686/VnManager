using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using VnManager.Services;

namespace VnManager
{
    public class DebugTester
    {
        public void Tester()
        {
            //RaiseException(13, 0, 0, new IntPtr(1));
            ///Globals.Logger.Error("something happened");

            LogManager.SetLogLevel(Services.LogLevel.Verbose);
            LogManager.Logger.Error("test DebugTester Entry");
        }
        


        
        [DllImport("kernel32.dll")]
        public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
