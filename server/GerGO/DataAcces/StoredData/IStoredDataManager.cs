using GerGO.Models;

namespace GerGO.DataAcces.StoredData
{
    interface IStoredDataManager
    {
        public string PrepareTable(string dbName, string tableName);
        public bool IsValidRow(string dbName, Table table, List<string> columnNames, ref string key, ref string value, ref int innerSeed);
        public void Insert(string dbName, string tableID, string key, string value);
        public void InsertToIndexFile(string dbName, string tableName, string mongoID, string pKey, string value);
        public void Delete(string dbName, string tableID, string key);
        public void DeleteFromIndexFile(string dbName, string tableName, string mongoID, string pKey);

        public void AddColumn(string dbName, string tableID, string value);
        public void RemoveColumn(string dbName, string tableID, bool isPkKey, int index);
        public void DropDatabase(string dbName);
        public void DropTable(string dbName, string mongoId);
        public List<string> GetAllRows(string dbName, string tableID);
        public string AddIndexFile(string dbName, string tableName);
        public void DropIndexFile(string colName, string mongoID);
    }
}
