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
    }
}
