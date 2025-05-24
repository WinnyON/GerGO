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
        [XmlArray("UniqueKeys")]
        [XmlArrayItem("UniqueKey")]
        public List<string> UniqueKeys { get; set; }
        [XmlArray("PrimaryKeys")]
        [XmlArrayItem("PKAttribute")]
        public List<PrimaryKey> PrimaryKeys { get; set; }

        public Table()
        {
            Name = string.Empty;
            MongoID = string.Empty;
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
            UniqueKeys = new List<string>();
            IndexFiles = new List<IndexFile>();
            PrimaryKeys = new List<PrimaryKey>();
        }

        public Table(string name)
        {
            Name = name;
            MongoID = string.Empty;
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
            UniqueKeys = new List<string>();
            IndexFiles = new List<IndexFile>();
            PrimaryKeys = new List<PrimaryKey>();
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
