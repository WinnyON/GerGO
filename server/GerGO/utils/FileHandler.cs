using GerGO.Models;

namespace GerGO.Utils
{
    interface FileHandler
    {
        public void WriteDataBaseData(string path, List<DataBase> dataBases);
        public List<DataBase> ReadDataBaseData(string path);
    }
}
