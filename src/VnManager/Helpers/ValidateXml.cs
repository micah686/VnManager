using System.Xml;
using System.Xml.Schema;

namespace VnManager.Helpers
{
    public static class ValidateXml
    {
        public static bool IsValidXml(string input)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                CheckCharacters = true,
                ConformanceLevel = ConformanceLevel.Document,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                ValidationFlags = XmlSchemaValidationFlags.None,
                ValidationType = ValidationType.None,
            };

            using (XmlReader xmlReader = XmlReader.Create(input, settings))
            {
                try
                {
                    while (xmlReader.Read()) { }                    
                    return true;
                }
                catch (XmlException)
                {
                    return false;
                }
            }
        }
    }
}
