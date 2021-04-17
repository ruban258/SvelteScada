using System.Xml.Serialization;

namespace GQLServer.Models
{
        public class XmlOpcUaVariable
    {
        [XmlAttribute("Name")]
        public string XmlVarLabel { get; set; }

        [XmlAttribute("DBName")]
        public string XmlS7db { get; set; }

        [XmlAttribute("VariableName")]
        public string XmlS7var { get; set; }

        [XmlAttribute("NodeID")]
        public string XmlNodeId { get; set; }

        [XmlAttribute("SamplingInterval")]
        public string XmlSamplingInterval { get; set; }
    }
    
}