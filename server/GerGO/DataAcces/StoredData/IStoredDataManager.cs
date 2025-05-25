using GerGO.Models;

namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public string PrepareTable(string dbName, string tableName);
        public void Insert(string dbName, string tableID, string key, string value);
        public void InsertToIndexFile(string dbName, string tableName, string mongoID, string pKey, string value);
        public void Delete(string dbName, string tableID, string key);
        public void DeleteFromIndexFile(string dbName, string tableName, string mongoID, string pKey);

        public void AddColumn(string dbName, string tableID, string value);
        public void RemoveColumn(string dbName, string tableID, bool isPkKey, int index);
        public void DropDatabase(string dbName);
        public void DropTable(string dbName, string mongoId);
        public List<string> GetAllRows(string dbName, string tableID);
        public string AddUniqueFile(string dbName, string tableName);
        public void InsertToUniqueFile(string dbName, string tableName, string mongoId, string pKey, string uKey);
        public void DeleteFromUniqueFile(string dbName, string tableName, string mongoID, string uKey);
        public string AddForeignKeyFile(string dbName, string tableName);
        public void InsertToForeignKeyFile(string dbName, string tableName, string mongoID, string pKey, string fKey);
        public void DeleteFromForeignKeyFile(string dbName, string tableName, string mongoID, string pKey, string fKeyVal);
        public string AddIndexFile(string dbName, string tableName);
        public void DropIndexFile(string colName, string mongoID);
        public bool ContainsValue(string dbName, string tableID,  int index, string value);
        public string GetValue(string dbName, string mongoID, string key);
        public List<string> GetValues(string dbName, string mongoID, List<string> keys);
        public List<string> GetPrimaryKeysWhere(string dbName, string mongoId, int colIndex, string type, string op, string val);
        public List<string> GetPrimaryKeysWhereAllRow(string dbName, string mongoId, int colIndex, string type, string op, string val);
        public bool ExistsKey(string dbName, string mongoID, string key);
        public void RemoveIndexAttribute(string colName, string mongoID, int index);
        public void UpdateIndexPKeyData(string colName, string mongoID, int index);
        public List<string> GetPrimaryKeys(string dbName, string mongoID);
    }
}
