using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace GQLServer.Models
{
    public class XmlOpcUaServer
    {
        [Required]
        [XmlAttribute("ServerName")]
        public string XmlServerlabel { get; set; }

        [Required]
        [XmlAttribute("URL")]
        public string XmlOpcUrl { get; set; }

        [XmlElement("UserName")]
        public string XmlUser { get; set; }

        [XmlElement("password")]
        public string XmlPassword { get; set; }

        // Subscription list on server.
        [XmlArrayItem("Subscription")]
        [XmlArray("Subscriptions")]
        public List<XmlOpcUaSubscription> XmlOpcUaSubscriptions { get; set; }
    }
    
}