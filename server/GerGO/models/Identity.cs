using System.Xml.Serialization;

namespace GerGO.Models
{
    public class Identity
    {
        [XmlAttribute("Seed")]
        public int Seed { get; set; }
        [XmlAttribute("Step")]
        public int Step { get; set; }

        public Identity()
        {
            Seed = 0;
            Step = 0;
        }

        public Identity(int seed, int step)
        {
            Seed = seed;
            Step = step;
        }
    }
}
