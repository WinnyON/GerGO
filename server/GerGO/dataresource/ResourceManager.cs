using GerGO.Models;

namespace GerGO.DataResource
{
    interface ResourceManager
    {
        public void AddDataBase(DataBase dataBase);
        public void DropDataBase(DataBase dataBase);
        public void AddTable(string dbName, Table table);
        public void DropTable(string dbName, Table table);
        public List<string[]> GetDBData();
        public void AddColumn(string dbName, string tableName, Column column);
        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey);
    }
}
