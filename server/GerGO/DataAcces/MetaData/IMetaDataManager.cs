using GerGO.Models;

namespace GerGO.DataAcces.MetaData
{
    interface IMetaDataManager
    {
        public bool ExitsDb(string dbName);
        public bool ExitsTable(string dbName, string tableName);
        public bool ExistsIndex(string dbName, string tableName, string indexName);
        public bool ExistsColumn(string dbName, string tableName, string columnName);
        public void AddDatabase(DataBase database);
        public void DropDatabase(DataBase database);
        public void AddTable(string dbName, string tableId, Table table);
        public void DropTable(string dbName, Table table);
        public void AddColumn(string dbName, string tableName, Column column);
        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public void AddIndex(string dbName, string tableName, string indexName, string columnName);
        public string GetTableMongoId(string dbName, string tableName);
        public string GetNextKey(string dbName, string tableName);
        // returns the list of the tables of a db
        public string[] GetTables(string dbName);
        // return all table of all db
        public List<string[]> GetDBData();
        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName);
        // return the column names
        public string[] GetColumns(string dbName, string tableName);
        public List<string[]> GetForeignKeys(string dbName, string tableName);
        public List<string> GetIndexData(string dbName, string tableName);

        public Table GetTable(string dbName, string tableName);

        public void UpdateInnerSeed(string dbName, string tableName, int value);
        public List<int> GetColumnPostions(string dbName, string tableName, List<string> columnNames);
    }
}
