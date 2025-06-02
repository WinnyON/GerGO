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

        public IndexFile(string name, List<string> attributes, string mongoID)
        {
            Name = name;
            Attributes = attributes;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is IndexFile index)
            {
                return this.Name == index.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
