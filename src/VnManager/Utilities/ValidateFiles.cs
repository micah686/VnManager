using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VnManager.Utilities
{
    public static class ValidateFiles
    {
        public static bool ValidateExe(string filepath)
        {
            try
            {
                if (!File.Exists(filepath)) return false;
                byte[] twoBytes = new byte[2];
                using (FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileStream.Read(twoBytes, 0, 2);
                }
                switch (Encoding.UTF8.GetString(twoBytes))
                {
                    case "MZ":
                        return true;
                    case "ZM":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Warning(ex, "Failed to validate executable");
                throw;
            }
        }

        public static bool EndsWithExe(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                string ext = Path.GetExtension(path).ToLower() ?? string.Empty;
                return ext.EndsWith(".exe");
            }
            catch (Exception ex)
            {
                LogManager.Logger.Warning(ex, "EndsWithExe Check Failed");
                return false;
            }
        }

        public static bool EndsWithIcoOrExe(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                string ext = Path.GetExtension(path).ToLower() ?? string.Empty;
                return ext.EndsWith(".ico") || ext.EndsWith(".exe");
            }
            catch (Exception ex)
            {
                LogManager.Logger.Warning(ex, "EndsWithIco check failed");
                return false;
            }
        }
    }
}
