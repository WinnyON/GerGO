using System.Xml.Serialization;

namespace GerGO.Models
{
    public class ForeignKey
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlElement("AttributeName")]
        public string AttributeName { get; set; }
        [XmlElement("RefTableName")]
        public string RefTableName { get; set; }
        [XmlElement("RefAttributeName")]
        public string RefAttributeName { get; set; }

        public ForeignKey()
        {
            Name = string.Empty;
            RefTableName = string.Empty;
            RefAttributeName = string.Empty;
            AttributeName = string.Empty;
        }

        public ForeignKey(string name, string mongoId, string refTableName, string refAttributeName, string attributeName)
        {
            Name = name;
            RefTableName = refTableName;
            RefAttributeName = refAttributeName;
            AttributeName = attributeName;
        }
        
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is ForeignKey fk)
            {
                return this.Name == fk.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
