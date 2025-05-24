using GerGO.Models;

namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public string PrepareTable(string dbName, string tableName);
        public bool IsValidRow(string dbName, Table table, List<string> columnNames, List<int> columnPositions, 
            ref string key, ref string value, ref int innerSeed);
        public void Insert(string dbName, string tableID, string key, string value);
        public void InsertToIndexFile(string dbName, string tableName, string mongoID, string pKey, string value);
        public void Delete(string dbName, string tableID, string key);
        public void DeleteFromIndexFile(string dbName, string tableName, string mongoID, string pKey);

        public void AddColumn(string dbName, string tableID, string value);
        public void RemoveColumn(string dbName, string tableID, bool isPkKey, int index, int nrPKeys);
        public void DropDatabase(string dbName);
        public void DropTable(string dbName, string mongoId);
        public List<string> GetAllRows(string dbName, string tableID);
        public string AddIndexFile(string dbName, string tableName);
        public void DropIndexFile(string colName, string mongoID);
        public void DropAllIndexes(string colName);
        public bool ContainsValue(string dbName, string tableID,  int index, string value);
        public string GetValue(string dbName, string mongoID, string key);
        public List<string> GetValues(string dbName, string mongoID, List<string> keys);
        public List<string> GetValuesWhere(string dbName, string mongoId, string type, string op, string val);
    }
}
