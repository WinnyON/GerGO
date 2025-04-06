using System.Xml.Serialization;

namespace GerGO.Models
{
    public class IndexFile
    {
        [XmlAttribute("Name")]
        public string Name {  get; set; }
        [XmlArray("Attributes")]
        [XmlArrayItem("Attribute")]
        public List<string> Attributes { get; set; }

        public IndexFile()
        {
            Name = string.Empty;
            Attributes = new List<string>();
        }

        public IndexFile(string name, List<string> attributes)
        {
            Name = name;
            Attributes = attributes;
        }
    }
}
