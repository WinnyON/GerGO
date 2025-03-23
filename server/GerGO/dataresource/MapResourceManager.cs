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
            _dataBases = new List<DataBase>();
            _dbDataSourceFile = "Catalog.xml";
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
    }
}
