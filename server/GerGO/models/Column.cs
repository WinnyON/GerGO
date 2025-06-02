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

        [XmlElement("Check")]
        public string Check {  get; set; }

        public Column()
        {
            Name = string.Empty;
            Type = string.Empty;
            NotNull = false;
            DefaultVal = string.Empty;
            Check = string.Empty;
        }

        public Column(string name, string type, bool notNull, string defaultVal, string check)
        {
            Name = name;
            Type = type;
            NotNull = notNull;
            DefaultVal = defaultVal;
            Check = check;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Column column)
            {
                return this.Name == column.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
