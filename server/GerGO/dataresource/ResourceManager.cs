using GerGO.Models;

namespace GerGO.DataResource
{
    interface ResourceManager
    {
        public void AddDataBase(DataBase dataBase);
        public void DropDataBase(DataBase dataBase);
    }
}
