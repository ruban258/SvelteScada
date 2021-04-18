using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GQLServer.Models;

namespace GQLServer.infra
{
    public class XMLParser
    {
        public XmlConfig OpcUaXmlFileRead(string path)
        {
            // Instance of the XmlSerializer.
            XmlSerializer serialzer = new XmlSerializer(typeof(XmlConfig));

            // Instance of filestrem and reader for the XML document.
            FileStream fs = new FileStream(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);

            // Declaration of the object which is to be created.
            XmlConfig config;

            // Filling of the object through deserialization.
            config = (XmlConfig)serialzer.Deserialize(reader);

            // Exit from Reader and Filestream.
            reader.Close();
            fs.Close();

            return config;
        }
    }
}