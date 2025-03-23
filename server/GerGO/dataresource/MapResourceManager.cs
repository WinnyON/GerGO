using GerGO.Models;
using GerGO.Utils;

namespace GerGO.DataResource
{
    class MapResourceManager : ResourceManager
    {
        private List<DataBase> _dataBases;
        private Logger _logger = LoggerFactory.GetLogger();
        private FileHandler _fileHandler = FileHandlerFactory.GetHandler();

        private string _dbDataSourceFile;

        public MapResourceManager()
        {
            _dbDataSourceFile = "Catalog.xml";
            //_dataBases = new List<DataBase>();
            _dataBases = _fileHandler.ReadDataBaseData(_dbDataSourceFile);
        }

        public void AddDataBase(DataBase dataBase)
        {
            if (_dataBases.Any((t) => (t.Name.Equals(dataBase.Name))))
            {
                _logger.Error("The database already exists!");
                throw new DataResourceException("The database already exists!");
            }

            try
            {
                _dataBases.Add(dataBase);
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to create db!");
            }
        }

        public void DropDataBase(DataBase dataBase)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dataBase.Name), null);
            if (result == null)
            {
                _logger.Error("The database doesn't exists!");
                throw new DataResourceException("The database doesn't exists!");
            }

            try
            {
                _dataBases.Remove(result);
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to delete db!");
            }
        }
    }
}
