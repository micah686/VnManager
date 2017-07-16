using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VisualNovelManagerv2.Design.Settings;

namespace VisualNovelManagerv2.CustomClasses
{
    public class ModifyUserSettings
    {
        public static void SaveUserSettings(UserSettings userSettings)
        {
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(UserSettings));
                StreamWriter myWriter = new StreamWriter(Globals.DirectoryPath + @"/Data/config/config.xml");
                mySerializer.Serialize(myWriter, userSettings);
                myWriter.Close();
                myWriter.Dispose();
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        public static UserSettings LoadUserSettings()
        {
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(UserSettings));
                FileStream fileStream = new FileStream(Globals.DirectoryPath + @"/Data/config/config.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                return (UserSettings) mySerializer.Deserialize(fileStream);
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }
    }
}
