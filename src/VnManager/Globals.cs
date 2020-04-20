using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace VnManager
{
    public static class Globals
    {
        public static readonly string DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool NsfwEnabled = false;
        public static LogLevel Loglevel = LogLevel.Normal;
        public static ILogger Logger;// created in boostrapper
    }

    public enum LogLevel
    {
        Normal,
        Debug,
        Verbose
    }
}
