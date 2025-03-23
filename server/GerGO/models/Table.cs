using System.Xml.Serialization;

namespace GerGO.Models
{
    public class Table
    {
        [XmlAttribute("TableName")]
        public string Name { get; set; }
        
        [XmlAttribute("FileName")]
        public string FileName { get; set; }
        
        [XmlAttribute("RowCount")]
        public int RowCount {  get; set; }
        
        [XmlArray("PrimaryKeys")]
        [XmlArrayItem("PrimaryKeyItem")]
        public List<string> PrinamryKeys { get; set; }
        
        [XmlArray("ForeignKeys")]
        [XmlArrayItem("ForeignKey")]
        public List<ForeignKey> ForeignKeys { get; set; }

        [XmlArray("Structure")]
        [XmlArrayItem("Column")]
        public List<Column> Columns { get; set; }

        public Table()
        {
            Name = string.Empty;
            FileName = string.Empty;
            RowCount = 0;
            PrinamryKeys = new List<string>();
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
        }

        public Table(string name)
        {
            Name = name;
            FileName = string.Empty;
            RowCount = 0;
            PrinamryKeys = new List<string>();
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
        }
        public Table(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
            RowCount = 0;
            PrinamryKeys = new List<string>();
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
        }
    }
}
