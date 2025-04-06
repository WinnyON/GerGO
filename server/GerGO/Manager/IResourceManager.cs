using GerGO.Models;

namespace GerGO.Manager
{
    interface IResourceManager
    {
        public void AddDataBase(DataBase dataBase);
        public void DropDataBase(DataBase dataBase);
        public void AddTable(string dbName, Table table);
        public void DropTable(string dbName, Table table);
        public void AddColumn(string dbName, string tableName, Column column);
        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public void AddIndexFile(string dbName, string tableName, IndexFile indexFile);
        // returns the list of the tables of a db
        public string[] GetTables(string dbName);
        //return all table of all db
        public List<string[]> GetDBData();
        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName);
        //return the column names
        public string[] GetColumns(string dbName, string tableName);
        public List<string[]> GetForeignKeys(string dbName, string tableName);
        //public void AddIndex(string dbName, string tableName, string columnName);
        public void Insert(string dbName, string tableName, string value);
        public void Delete(string dbName, string tableName, string key);
        public List<string>GetAllRows(string dbName, string tableName);
    }
}
