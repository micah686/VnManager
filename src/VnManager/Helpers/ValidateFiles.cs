// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace VnManager.Helpers
{
    public static class ValidateFiles
    {
        /// <summary>
        /// Validates that a given file is a valid executable
        /// </summary>
        /// <param name="filepath">Path to the file to validate</param>
        /// <returns>Returns true is the exe is valid</returns>
        public static bool ValidateExe(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    return false;
                }
                byte[] twoBytes = new byte[2];
                int bytesRead = 0;
                using (FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bytesRead = fileStream.Read(twoBytes, 0, twoBytes.Length);
                }
                if (bytesRead != twoBytes.Length)
                {
                    return false;
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
                App.Logger.Warning(ex, "Failed to validate executable");
                throw;
            }
        }

        /// <summary>
        /// Checks if the path ends with .exe
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Returns true if the file ends with .exe</returns>
        public static bool EndsWithExe(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }
                string ext = Path.GetExtension(path).ToUpperInvariant() ?? string.Empty;
                return ext.EndsWith(".EXE");
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "EndsWithExe Check Failed");
                return false;
            }
        }

        /// <summary>
        /// Checks if the path ends with .ico or .exe
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Returns true if the file ends with .ico or .exe</returns>
        public static bool EndsWithIcoOrExe(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }
                string ext = Path.GetExtension(path).ToUpperInvariant() ?? string.Empty;
                return ext.EndsWith(".ICO") || ext.EndsWith(".EXE");
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "EndsWithIco check failed");
                return false;
            }
        }

        /// <summary>
        /// Ends with .jpg or .png
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool EndsWithJpgOrPng(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }
                string ext = Path.GetExtension(path).ToUpperInvariant() ?? string.Empty;
                return ext.EndsWith(".JPG") || ext.EndsWith(".PNG");
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "EndsWithIco check failed");
                return false;
            }
        }
    }
}
