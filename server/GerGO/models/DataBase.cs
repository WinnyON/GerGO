using System.Xml.Serialization;

namespace GerGO.Models
{
    [XmlRoot("DataBase")]
    public class DataBase
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlArray("Tables")]
        [XmlArrayItem("Table")]
        public List<Table> Tables {  get; set; }

        public DataBase()
        {
            Name = string.Empty;
            Tables = new List<Table>();
        }

        public DataBase(string name)
        {
            Name = name;
            Tables = new List<Table>();
        }
    }
}
