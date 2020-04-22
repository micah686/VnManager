using System.IO;
using System.Reflection;

namespace VnManager
{
    public static class Globals
    {
        public static readonly string DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool IsNsfwEnabled { get; set; } = false;
    }
}
