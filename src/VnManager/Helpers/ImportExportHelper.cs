// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AdysTech.CredentialManager;
using LiteDB;
using LiteDB.Engine;

namespace VnManager.Helpers
{
    public class ImportExportHelper
    {
        public static void CreateBackup()
        {
            Compact(App.AssetDirPath);
            Expand(Path.Combine(App.AssetDirPath, "example.vnbak"));
        }

        public static bool Compact(string outputFile)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return false;
                }
                File.Copy(Path.Combine(App.ConfigDirPath, App.DbPath), @$"{App.AssetDirPath}\Import.db");
                var fileName = @$"{App.AssetDirPath}\Import.db";
                using (var db = new LiteDatabase($"Filename={fileName};Password={cred.Password}"))
                {
                    db.Rebuild(new RebuildOptions { Password = App.ImportExportDbKey });
                }


                var backup = new BackupFormat();
                ZipFile.CreateFromDirectory(@$"{App.AssetDirPath}\sources", @$"{App.AssetDirPath}\Images.zip");
                using (ZipArchive archive = ZipFile.Open(@$"{App.AssetDirPath}\output.zip", ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(@$"{App.AssetDirPath}\Images.zip", "Images.zip");
                    archive.CreateEntryFromFile(@$"{App.AssetDirPath}\Import.db", "Import.db");
                }
                File.Delete(@$"{App.AssetDirPath}\Images.zip");
                File.Delete(@$"{App.AssetDirPath}\Import.db");

                byte[] originalBytes = File.ReadAllBytes(@$"{App.AssetDirPath}\output.zip");
                File.Delete(@$"{App.AssetDirPath}\output.zip");

                backup.ZippedData = originalBytes;


                using (var fs = new FileStream(outputFile, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
                {
                    using (var writer = new BinaryWriter(fs, Encoding.UTF8, true))
                    {
                        writer.Write(backup.HeaderBytes);
                        writer.Write(backup.ZippedData);

                    }
                }

                return true;
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to compact database");
                throw;
            }

        }

        public static bool Expand(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                return false;
            }

            if (File.Exists(@$"{App.AssetDirPath}\Import.db"))
            {
                File.Delete(@$"{App.AssetDirPath}\Import.db");
            }
            if (File.Exists(@$"{App.AssetDirPath}\Images.zip"))
            {
                File.Delete(@$"{App.AssetDirPath}\Images.zip");
            }

            var input = File.ReadAllBytes(inputFile);
            char[] header = Encoding.UTF8.GetString(input.Take(7).ToArray()).ToCharArray();
            var headerStr = new string(header);
            if (headerStr != "VnMgBak")
            {
                return false;
            }
            var data = input.Skip(7);


            const double thresholdRatio = 10;
            using (var ms = new MemoryStream(data.ToArray()))
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    var dbEntry = archive.Entries.FirstOrDefault(x => x.Name == "Import.db");
                    if (dbEntry != null)
                    {
                        var compressionRatio = dbEntry.Length / dbEntry.CompressedLength;
                        if (compressionRatio > thresholdRatio)
                        {
                            return false;
                        }

                        dbEntry.ExtractToFile(@$"{App.AssetDirPath}\Import.db");
                    }
                    var imageEntry = archive.Entries.FirstOrDefault(x => x.Name == "Images.zip");
                    if (imageEntry != null)
                    {
                        var compressionRatio = imageEntry.Length / imageEntry.CompressedLength;
                        if (compressionRatio > thresholdRatio)
                        {
                            return false;
                        }
                        imageEntry.ExtractToFile(@$"{App.AssetDirPath}\Images.zip");
                    }
                }
            }
            return true;
        }
        
        private class BackupFormat
        {
            //Header
            public char[] HeaderBytes { get; set; } = { 'V', 'n', 'M', 'g', 'B', 'a', 'k' };
            //Content
            public byte[] ZippedData { get; set; }

        }


    }

    
}
