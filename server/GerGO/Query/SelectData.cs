namespace GerGO.Query
{
    class SelectData
    {
        public string DbName { get; set; }
        public string TableName { get; set; }
        public List<string[]> Columns { get; set; }
        public List<string[]> JoinTables { get; set; }
        public List<string[]> WhereClauses { get; set; }
        public List<string[]> GroupByClauses { get; set; }
        public List<string[]> HavingClauses { get; set; }
        public List<string[]> OrderByCluases { get; set; }
        public List<string[]> AggFunctions { get; set; }
        public bool Distinct { get; set; }
        public int Limit { get; set; }
        public SelectData()
        {
            DbName = string.Empty;
            TableName = string.Empty;
            Columns = [];
            JoinTables = [];
            WhereClauses = [];
            GroupByClauses = [];
            HavingClauses = [];
            OrderByCluases = [];
            AggFunctions = [];
            Distinct = false;
            Limit = -1;
        }
    }
}
