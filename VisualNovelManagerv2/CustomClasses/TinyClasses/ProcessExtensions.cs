using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace VisualNovelManagerv2.CustomClasses.TinyClasses
{
    public static class ProcessExtensions
    {
        public static List<Process> GetChildProcesses(this Process process)
        {
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher(
                    $"Select * From Win32_Process Where ParentProcessID={process.Id}");

                return (from ManagementObject mo in mos.Get() select Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]))).ToList();
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }
    }
}
