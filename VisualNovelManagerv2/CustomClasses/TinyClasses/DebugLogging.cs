using System;
using System.IO;

namespace VisualNovelManagerv2.CustomClasses.TinyClasses
{
    public class DebugLogging
    {
        public static void WriteDebugLog(Exception ex)
        {
            string strPath = $@"{Globals.DirectoryPath}\Debug.log";
            if (!File.Exists(strPath))
            {
                File.Create(strPath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine("=============Error Logging ===========");
                sw.WriteLine("===========Start=============");
                sw.WriteLine(DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
                sw.WriteLine(DateTime.Now);
                sw.WriteLine("===========End============= ");
                sw.WriteLine("\n\n");
            }
        }
    }
}
