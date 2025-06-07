using GerGO.Models;

namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public void Insert<T>(string dbName, string collectionName, List<MongoEntity<T>> data);
        public void Delete<T>(string dbName, string tableName, List<T> pKeys);
        public void AddColumn(string dbName, string tableName, string value);
        public void DropDatabase(string dbName);
        public void DropTable(string dbName, string tableName);
        public List<string> GetAllRows(string dbName, string tableName);
        public void InsertIndexData<T>(string dbName, string collectionName, List<MongoEntity<T>> data);
        public bool ExistsKey<T>(string dbName, string collectionName, T key);
        public void RemoveColumn(string dbName, string tableName, int index);
        public void DeleteIndexData(string dbName, string collectionName, List<string> pKeys);
        public void DeleteUniqueIndexData(string dbName, string collectionName, List<string> pKeys);
        public List<string> GetAllKeys(string dbName, string tableName);
        public List<string> GetKeysWhere<T>(string dbName, string collectionName, string op, T val);
        public List<string> Get_idWhere<T>(string dbName, string collectionName, string op, T val);
        public List<string> GetKeysWhereIter(string dbName, string tableName, int colIndex, string type, string op, string val);
        public List<string> GetRows<T>(string dbName, string tableName, List<T> keys);
        public List<string> GetRows<T>(string dbName, string tableName, List<T> keys, List<int> colIndexes);
        public Dictionary<string, List<string>> GetBuildSide<T>(string dbName, string tableName, List<T> pKeys, int KeyIndex, List<int> colIndexes);
        public int UpdateColumn<T>(string dbName, string tableName, int index, string newVal, List<T> pKeys);

        //public string GetValue(string dbName, string tableName, string key);
        //public string GetFullRow(string dbName, string mongoID, string key);
        //public List<string> GetValues(string dbName, string mongoID, List<string> keys);
    }
}
