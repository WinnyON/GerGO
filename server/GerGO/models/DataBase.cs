namespace GerGO.models
{
    class DataBase
    {
        public string Name { get; set; }

        private Dictionary<string, Table> _tables;

        public DataBase()
        {
            Name = string.Empty;
            _tables = new Dictionary<string, Table>();
        }

        public DataBase(string name)
        {
            Name = name;
            _tables = new Dictionary<string, Table>();
        }
    }
}
