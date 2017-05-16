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
        public static readonly string connectionString = @"Data Source=|DataDirectory|\Data\Database\Database.db;" +
                                                          "Version=3;" + "Pooling=True;" + "Max Pool Size=5;" +
                                                          "Page Size=4096;" +
                                                          "Cache Size=4000;" + "PRAGMA foreign_keys = ON;";
        public static readonly string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly object writeLock = new object();
    }
}
