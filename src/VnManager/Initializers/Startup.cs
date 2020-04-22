using System.IO;

namespace VnManager.Initializers
{
    public class Startup
    {
        public static void CreatFolders()
        {
            //Main folders
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Main\config");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Main\Database");            
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Main\libs\");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Main\logs");
            //Main resources
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Main\res\icons");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Main\res\icons\country_flags");
            //vndb
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Vndb\images\cover");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Vndb\images\screenshots");
            Directory.CreateDirectory(Globals.DirectoryPath + @"\Data\Vndb\images\characters");

        }
    }
}
