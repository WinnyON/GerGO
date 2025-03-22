namespace GerGO.models
{
    enum DataType { VOID, BIT, INT, FLOAT, STRING, DATE, DATETIME }
    class Column
    {
        public string Name { get; set; }
        public DataType Type { get; set; }
        public bool NotNull { get; set; }
        public object DefaultVal { get; set; }
        public ValueTuple<int, int> Identity { get; set; }
        public bool Unique { get; set; }

        public Column()
        {
            Name = string.Empty;
            Type = DataType.VOID;
            NotNull = false;
            DefaultVal = null;
            Identity = new ValueTuple<int, int>(0, 0);
            Unique = false;
        }

        public Column(string name, DataType type, bool notNull, object defaultVal, ValueTuple<int, int> identity, bool unique)
        {
            Name = name;
            Type = type;
            NotNull = notNull;
            DefaultVal = defaultVal;
            Identity = identity;
            Unique = unique;
        }
    }
}
