using System.Xml.Serialization;

namespace GerGO.Models
{
    public enum DataType { VOID, BIT, INT, FLOAT, STRING, DATE, DATETIME }

    public class Column
    {
        [XmlAttribute("FieldName")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public DataType Type { get; set; }
        
        [XmlAttribute("NotNull")]
        public bool NotNull { get; set; }
        
        [XmlAttribute("DefaultVal")]
        public string DefaultVal { get; set; }
        
        [XmlElement("Identity")]
        public Identity PKIdentity{ get; set; }
        
        [XmlAttribute("Unique")]
        public bool Unique { get; set; }

        public Column()
        {
            Name = string.Empty;
            Type = DataType.VOID;
            NotNull = false;
            DefaultVal = string.Empty;
            PKIdentity = new Identity(0, 0);
            Unique = false;
        }

        public Column(string name, DataType type, bool notNull, string defaultVal, Identity identity, bool unique)
        {
            Name = name;
            Type = type;
            NotNull = notNull;
            DefaultVal = defaultVal;
            PKIdentity = identity;
            Unique = unique;
        }
    }
}
