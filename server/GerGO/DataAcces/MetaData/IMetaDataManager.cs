using GerGO.Models;

namespace GerGO.DataAcces.MetaData
{
    interface IMetaDataManager
    {
        public bool ExistsDb(string dbName);
        public bool ExistsTable(string dbName, string tableName);
        public bool ExistsIndex(string dbName, string tableName, string indexName);
        public bool ExistsColumn(string dbName, string tableName, string columnName);
        public bool ExistsForeignKey(string dbName, string tableName, string foreignKey);
        public bool HasFkConstraint(string dbName, string tableName, string columnName);
        public void AddDatabase(DataBase database);
        public void DropDatabase(DataBase database);
        public void AddTable(string dbName, string tableId, Table table);
        public void DropTable(string dbName, Table table);
        public void AddColumn(string dbName, string tableName, Column column, PrimaryKey? pKey, UniqueKey? uKey);
        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public void AddIndex(string dbName, string tableName, IndexFile iFile);
        public void DropIndex(string dbName, string tableName, IndexFile index);
        public string GetTableMongoId(string dbName, string tableName);
        // returns the list of the tables of a db
        public string[] GetTables(string dbName);
        // return all table of all db
        public List<string[]> GetDBData();
        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName);
        // return the column names
        public string[] GetColumns(string dbName, string tableName);
        List<string> GetPrimaryKeys(string dbName, string tableName);
        public List<string[]> GetForeignKeys(string dbName, string tableName);
        public List<string> GetIndexData(string dbName, string tableName);

        public Table GetTable(string dbName, string tableName);
        public Column GetColumn(string dbName, string tableName, string columnName);
        public IndexFile GetIndexFile(string dbName, string tableName, string indexFileName);
        public ForeignKey GetForeignKey(string dbName, string tableName, string fkName);

        public List<int> GetColumnPostions(string dbName, string tableName, List<string> columnNames);
        public int GetNrPkeys(string dbName, string tableName);
        public void DropColumn(string dbName, string tableName, Column column);
        public void DropForeignKey(string dbName, string tableName, ForeignKey foreignKey);
        public string HasIndexOnIt(string dbName, string tableName, string columnName);

        

        public string GetPrimaryKey(string dbName, Table table, string row, List<string> columnNames);
        public string GetValuePart(Table table, string row, List<string> columnNames);
        public List<string> GetReferingForeignKeys(string dbName, string tableName);
    }
}
