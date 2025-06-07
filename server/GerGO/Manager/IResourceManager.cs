using GerGO.Models;
using GerGO.Query;

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
        public void DropColumn(string dbName, string tableName, string colName);
        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public void DropForeignKey(string dbName, string tableName, string fkName);
        public void AddIndexFile(string dbName, string tableName, IndexFile iFile);
        // returns the list of the tables of a db
        public string[] GetTables(string dbName);
        //return all table of all db
        public List<string[]> GetDBData();
        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName);
        //return the column names
        public List<string[]> GetForeignKeys(string dbName, string tableName);
        public List<string> GetIndexes(string dbName, string tableName);
        public int Insert(string dbName, string tableName, List<string> columnNames, List<string> rows);
        public int Delete(string dbName, string tableName, List<string> keys);
        public List<string> GetAllRows(string dbName, string tableName, List<string> columnNames);

        public List<string> Select(SelectData selectData, ref string columnNames);
        public int DeleteWhere(string dbName, string tableName, List<string[]> wheres);
        public int Update(string dbName, string tableName, List<string[]> wheres, string colName, string newVal);
    }
}
