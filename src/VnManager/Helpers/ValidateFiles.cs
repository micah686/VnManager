using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VnManager.Utilities;

namespace VnManager.Helpers
{
    public static class ValidateFiles
    {
        public static bool ValidateExe(string filepath)
        {
            try
            {
                if (!File.Exists(filepath)) return false;
                byte[] twoBytes = new byte[2];
                int bytesRead = 0;
                using (FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bytesRead = fileStream.Read(twoBytes, 0, 2);
                }
                if (bytesRead != 2)
                    return false;
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
