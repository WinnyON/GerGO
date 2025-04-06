namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public string PrepareTable(string dbName, string tableName);
        public void Insert(string dbName, string tableID, string key, string value);
        public void Delete(string tableID, string key);
    }
}
