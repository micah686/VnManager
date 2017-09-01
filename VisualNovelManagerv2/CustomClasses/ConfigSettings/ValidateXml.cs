using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace VisualNovelManagerv2.CustomClasses.ConfigSettings
{
    public class ValidateXml
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
                    while (xmlReader.Read())
                    {
                         // This space intentionally left blank
                    }
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
