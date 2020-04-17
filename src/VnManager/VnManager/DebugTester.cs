using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace VnManager
{
    public class DebugTester
    {
        public void Tester()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }
        
        
        [DllImport("kernel32.dll")]
        public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
