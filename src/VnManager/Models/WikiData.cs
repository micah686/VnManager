// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace VnManager.Models
{
    [XmlRoot(ElementName = "api")]
    public class WikiDataApi
    {

        [XmlElement(ElementName = "entities")]
        public WdEntities WdEntities { get; set; }

        [XmlAttribute(AttributeName = "success")]
        public int Success { get; set; }
    }

    [XmlRoot(ElementName = "entities")]
    public class WdEntities
    {

        [XmlElement(ElementName = "entity")]
        public WdEntity WdEntity { get; set; }
    }
    
    [XmlRoot(ElementName = "entity")]
    public class WdEntity
    {

        [XmlElement(ElementName = "sitelinks")]
        public WdSitelinks WdSitelinks { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "sitelinks")]
    public class WdSitelinks
    {

        [XmlElement(ElementName = "sitelink")]
        public WdSitelink WdSitelink { get; set; }
    }
    
    [XmlRoot(ElementName = "sitelink")]
    public class WdSitelink
    {

        [XmlElement(ElementName = "badges")]
        public object Badges { get; set; }

        [XmlAttribute(AttributeName = "site")]
        public string Site { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }
}
