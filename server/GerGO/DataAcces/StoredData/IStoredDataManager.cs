namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public string PrepareTable(string dbName, string tableName);
        public bool IsValidRow(string dbName, string tableName, List<string[]> columns, ref string value);
        public void Insert(string dbName, string tableID, string key, string value);
        public void Delete(string dbName, string tableID, string key);

        public void AddColumn(string dbName, string tableID, string value);
        public void DropDatabase(string dbName);
        public void DropTable(string dbName, string mongoId);
        public List<string> GetAllRows(string dbName, string tableID);
    }
}
