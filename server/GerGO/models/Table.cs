using System.Xml.Serialization;

namespace GerGO.Models
{
    public class Table
    {
        [XmlAttribute("TableName")]
        public string Name { get; set; }
        
        [XmlAttribute("MongoID")]
        public string MongoID { get; set; }
        
        [XmlArray("ForeignKeys")]
        [XmlArrayItem("ForeignKey")]
        public List<ForeignKey> ForeignKeys { get; set; }
        [XmlArray("IndexFiles")]
        [XmlArrayItem("Index")]
        public List<IndexFile> IndexFiles { get; set; }

        [XmlArray("Structure")]
        [XmlArrayItem("Column")]
        public List<Column> Columns { get; set; }

        public Table()
        {
            Name = string.Empty;
            MongoID = string.Empty;
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
            IndexFiles = new List<IndexFile>();
        }

        public Table(string name)
        {
            Name = name;
            MongoID = string.Empty;
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
            IndexFiles = new List<IndexFile>();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Table table)
            {
                return this.Name == table.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
