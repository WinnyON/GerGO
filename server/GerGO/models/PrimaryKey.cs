using System.Xml.Serialization;

namespace GerGO.Models
{
    public class PrimaryKey
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlElement("Identity")]
        public Identity PKIdentity { get; set; }

        public PrimaryKey()
        {
            Name = string.Empty;
            PKIdentity = new Identity();
        }

        public PrimaryKey(string name, int seed, int step)
        {
            Name = name;
            PKIdentity = new Identity();
            PKIdentity.Seed = seed;
            PKIdentity.Step = step;
        }

        public PrimaryKey(string name, Identity pKIdentity)
        {
            Name = name;
            PKIdentity = pKIdentity;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is PrimaryKey pKey)
            {
                return this.Name == pKey.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
