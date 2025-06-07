namespace GerGO.Models
{
    public class MongoEntity<T>
    {
        public T Key { get; set; }
        public string Value { get; set; }

        public MongoEntity()
        {
            Key = default;
            Value = string.Empty;
        }

        public MongoEntity(T key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
