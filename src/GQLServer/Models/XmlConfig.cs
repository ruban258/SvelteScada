using System.Xml.Serialization;

namespace GQLServer.Models
{
    public class XmlConfig
    {
        public XmlConfig()
        {
            XmlOpcUaServer = new XmlOpcUaServer();
        }

        [XmlElement("OPCUAServer")]
        public XmlOpcUaServer XmlOpcUaServer { get; set; }

    }
}