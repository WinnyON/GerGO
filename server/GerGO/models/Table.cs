namespace GerGO.models
{
    class Table
    {
        public string Name { get; set; }
        private int _rowCount = 0;
        private List<string> _prinamryKeys;
        private Dictionary<string, ForeignKey> _foreignKeys;
        private Dictionary<string, Column> _columns;

        public Table()
        {
            Name = string.Empty;
            _rowCount = 0;
            _prinamryKeys = new List<string>();
            _foreignKeys = new Dictionary<string, ForeignKey>();
            _columns = new Dictionary<string, Column>();
        }

        public Table(string name)
        {
            Name = name;
            _rowCount = 0;
            _prinamryKeys = new List<string>();
            _foreignKeys = new Dictionary<string, ForeignKey>();
            _columns = new Dictionary<string, Column>();
        }
    }
}
