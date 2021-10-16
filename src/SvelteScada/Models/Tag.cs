using Opc.Ua;

namespace GQLServer.Models
{
    public class Tag
    {
        public string TagName { get; set; }
        string NodeId { get; set; }
        string NodeName { get; set; }
        public string Value { get; set; }
        public string StatusCode { get; set; }
        public BuiltInType TagType {get; set;}        
    }
}