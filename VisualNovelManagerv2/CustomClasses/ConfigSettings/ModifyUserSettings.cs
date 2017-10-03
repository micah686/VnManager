using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.Design.Settings;

namespace VisualNovelManagerv2.CustomClasses.ConfigSettings
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

                using (FileStream fileStream = new FileStream(Globals.DirectoryPath + @"/Data/config/config.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool isValid = ValidateXml.IsValidXml(Globals.DirectoryPath + @"/Data/config/config.xml");
                    return isValid == true ? (UserSettings) mySerializer.Deserialize(fileStream) : null;
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        public static void RemoveUserSettingsNode(int id)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                using (FileStream fileStream = new FileStream(Globals.DirectoryPath + @"/Data/config/config.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    doc.Load(fileStream);
                    string node = $"/UserSettings/VnSetting[Id={id}]";
                    XmlNode nodeToDelete = doc.SelectSingleNode(node);
                    nodeToDelete?.ParentNode?.RemoveChild(nodeToDelete);
                    fileStream.Dispose();
                }
                doc.Save(Globals.DirectoryPath + @"/Data/config/config.xml");
            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }
            
        }
    }
}
