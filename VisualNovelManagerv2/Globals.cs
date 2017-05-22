using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2
{
    public static class Globals
    {
        public static readonly string ConnectionString = @"Data Source=|DataDirectory|\Data\Database\Database.db;" +
                                                          "Version=3;" + "Pooling=True;" + "Max Pool Size=5;" +
                                                          "Page Size=4096;" +
                                                          "Cache Size=4000;" + "PRAGMA foreign_keys = ON;";
        public static readonly string DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly object WriteLock = new object();
        public static readonly List<string> ClientInfo = new List<string>{"Visual Novel Manager v2", "0.0.1"};
        public static int VnId;
    }
}
