using System.Xml.Serialization;

namespace GerGO.Models
{
    public enum DataType { VOID, BIT, INT, FLOAT, STRING, DATE, DATETIME }

    public class Column
    {
        [XmlAttribute("FieldName")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }
        
        [XmlAttribute("NotNull")]
        public bool NotNull { get; set; }
        
        [XmlElement("DefaultVal")]
        public string DefaultVal { get; set; }
        
        [XmlElement("PrimaryKey")]
        public bool PrimaryKey{ get; set; }

        [XmlAttribute("Identity")]
        public bool Identity { get; set; }
        
        [XmlAttribute("Unique")]
        public bool Unique { get; set; }

        [XmlElement("Check")]
        public string Check {  get; set; }

        public Column()
        {
            Name = string.Empty;
            Type = string.Empty;
            NotNull = false;
            DefaultVal = string.Empty;
            PrimaryKey = false;
            Identity = false;
            Unique = false;
            Check = string.Empty;
        }

        public Column(string name, string type, bool notNull, string defaultVal, bool primaryKey, bool identity, bool unique, string check)
        {
            Name = name;
            Type = type;
            NotNull = notNull;
            DefaultVal = defaultVal;
            PrimaryKey = primaryKey;
            Identity = identity;
            Unique = unique;
            Check = check;
        }
    }
}
