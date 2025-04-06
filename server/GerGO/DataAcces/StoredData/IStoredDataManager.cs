namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public string PrepareTable(string dbName, string tableName);
        public void Insert(string dbName, string tableID, string key, string value);
        public void Delete(string dbName, string tableID, string key);
        public List<string> GetAllRows(string dbName, string tableID);
    }
}
