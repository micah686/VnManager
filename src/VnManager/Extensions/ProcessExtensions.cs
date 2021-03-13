// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Sentry;
using VnManager.Helpers;

namespace VnManager.Extensions
{
    public static class ProcessExtensions
    {
        public static IEnumerable<Process> GetChildProcesses(this Process process)
        {
            try
            {
                if (process == null)
                {
                    return new List<Process>();
                }
                ManagementObjectSearcher mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");

                var childList = (from ManagementObject mo in mos.Get() select Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]))).ToList();
                return childList;
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to get child processes");
                SentryHelper.SendException(ex, null, SentryLevel.Error);
                throw;
            }
        }
    }
}
