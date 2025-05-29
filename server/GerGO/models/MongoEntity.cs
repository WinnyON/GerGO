namespace GerGO.Models
{
    public class MongoEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public MongoEntity()
        {
            Key = string.Empty;
            Value = string.Empty;
        }

        public MongoEntity(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
