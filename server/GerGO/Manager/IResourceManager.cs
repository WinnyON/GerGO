using GerGO.Models;

namespace GerGO.Manager
{
    interface IResourceManager
    {
        public void AddDataBase(DataBase dataBase);
        public void DropDataBase(DataBase dataBase);
        public void AddTable(string dbName, Table table);
        public void DropTable(string dbName, Table table);
        public void DropIndex(string dbName, string tableName, string indexName);
        public void AddColumn(string dbName, string tableName, Column column, PrimaryKey? pKey, bool isUnique);
        public void DropColumn(string dbName, string tableName, Column column);
        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public void DropForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public void AddIndexFile(string dbName, string tableName, string indexName, string columnName);
        // returns the list of the tables of a db
        public string[] GetTables(string dbName);
        //return all table of all db
        public List<string[]> GetDBData();
        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName);
        //return the column names
        public string[] GetColumns(string dbName, string tableName);
        public List<string[]> GetForeignKeys(string dbName, string tableName);
        public List<string> GetIndexes(string dbName, string tableName);
        public void Insert(string dbName, string tableName, List<string> columnNames, string value);
        public void Delete(string dbName, string tableName, string key);
        public List<string> GetAllRows(string dbName, string tableName, List<string> columnNames);

        public List<string> Select(SelectData selectData, ref string columnNames);
        public void DeleteWhere(string dbName, string tableName, List<string[]> wheres);
    }
}
