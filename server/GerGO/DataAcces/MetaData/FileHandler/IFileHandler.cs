using GerGO.Models;

namespace GerGO.DataAcces.MetaData.FileHandler
{
    interface IFileHandler
    {
        public void WriteDataBaseData(string path, List<DataBase> dataBases);
        public List<DataBase> ReadDataBaseData(string path);
    }
}
