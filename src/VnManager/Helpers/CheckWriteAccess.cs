using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VnManager.Helpers
{
    public static class CheckWriteAccess
    {
        public static bool CheckWrite(string dirPath)
        {
            var testfile = Path.Combine(dirPath, Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(testfile, "");
                File.Delete(testfile);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
