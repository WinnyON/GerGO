using GerGO.Models;

namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public void Insert(string dbName, string collectionName, List<MongoEntity> data);
        public void Delete(string dbName, string tableName, List<string> pKeys);
        public void AddColumn(string dbName, string tableName, string value);
        public void DropDatabase(string dbName);
        public void DropTable(string dbName, string tableName);
        public List<string> GetAllRows(string dbName, string tableName);
        public void InsertIndexData(string dbName, string collectionName, List<MongoEntity> data);
        public bool ExistsKey(string dbName, string collectionName, string key);
        public void RemoveColumn(string dbName, string tableName, int index);
        public void DeleteIndexData(string dbName, string collectionName, List<string> pKeys);
        public void DeleteUniqueIndexData(string dbName, string collectionName, List<string> pKeys);
        public List<string> GetAllPrimaryKeys(string dbName, string tableName);
        public List<string> GetKeysWhere(string dbName, string collectionName, string op, string val);
        public List<string> Get_idWhere(string dbName, string collectionName, string op, string val);
        public List<string> GetKeysWhereIter(string dbName, string tableName, int colIndex, string type, string op, string val);


        //public string GetValue(string dbName, string tableName, string key);
        //public string GetFullRow(string dbName, string mongoID, string key);
        //public List<string> GetValues(string dbName, string mongoID, List<string> keys);
    }
}
