using System.Xml.Linq;
using System.Xml.Serialization;

namespace GerGO.Models
{
    public class UniqueKey
    {
        [XmlAttribute("Column")]
        public string Column {  get; set; }

        [XmlAttribute("MongoID")]
        public string MongoID {  get; set; }

        public UniqueKey()
        {
            Column = string.Empty;
            MongoID = string.Empty;
        }

        public UniqueKey(string column, string mongoID)
        {
            Column = column;
            MongoID = mongoID;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is UniqueKey uKey)
            {
                return this.Column == uKey.Column;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Column.GetHashCode();
        }
    }
}
