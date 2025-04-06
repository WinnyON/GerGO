using System.Xml.Serialization;

namespace GerGO.Models
{
    public class ForeignKey
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("RefTableName")]
        public string RefTableName { get; set; }
        [XmlElement("RefAttributeName")]
        public string RefAttributeName { get; set; }
        [XmlElement("AttributeName")]
        public string AttributeName { get; set; }

        public ForeignKey()
        {
            Name = string.Empty;
            RefTableName = string.Empty;
            RefAttributeName = string.Empty;
            AttributeName = string.Empty;
        }

        public ForeignKey(string name, string refTableName, string refAttributeName, string attributeName)
        {
            Name = name;
            RefTableName = refTableName;
            RefAttributeName = refAttributeName;
            AttributeName = attributeName;
        }
    }
}
