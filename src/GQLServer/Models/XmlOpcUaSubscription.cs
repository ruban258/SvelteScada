using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace GQLServer.Models
{
    public class XmlOpcUaSubscription
    {
        public XmlOpcUaSubscription()
        {
            XmlOpcUaVariables = new List<XmlOpcUaVariable>();
        }

        [Required]
        [XmlAttribute("PublishingInterval")]
        public string XmlPublishingInterval { get; set; }

        [XmlArrayItem("OPCVariable")]
        [XmlArray("OPCVariables")]
        public List<XmlOpcUaVariable> XmlOpcUaVariables { get; set; }
    }
}