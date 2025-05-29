using System.Xml.Serialization;

namespace GerGO.Models
{
    public class PrimaryKey
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlElement("IdentitySeed")]
        public Int64 IdentitySeed { get; set; }
        [XmlElement("IdentityStep")]
        public Int64 IdentityStep { get; set; }
        [XmlElement("IdentityInnerSeed")]
        public Int64 IdentityInnerSeed { get; set; }

        public PrimaryKey()
        {
            Name = string.Empty;
            IdentitySeed = 0;
            IdentityInnerSeed = 0;
            IdentityStep = 0;
        }

        public PrimaryKey(string name, int seed, int step)
        {
            Name = name;
            IdentitySeed = seed;
            IdentityStep = step;
            IdentityInnerSeed = seed;
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
