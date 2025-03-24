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
                _dataBases.Remove(dataBase);
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
                _dataBases.Add(result);
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to delete db!");
            }
        }
        public void AddTable(string dbName, Table table)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dbName), null);
            if (result == null)
            {
                _logger.Error("The database doesn't exists!");
                throw new DataResourceException("The database doesn't exists!");
            }

            Table resTable = result.Tables.FirstOrDefault((t) => t.Name.Equals(table.Name), null);
            if (resTable != null)
            {
                _logger.Error("The table already exists!");
                throw new DataResourceException("The table already exists!");
            }

            try
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.Add(table);
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.Remove(table);
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to create table!");
            }
        }

        public void DropTable(string dbName, Table table)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dbName), null);
            if (result == null)
            {
                _logger.Error("The database doesn't exists!");
                throw new DataResourceException("The database doesn't exists!");
            }

            Table resTable = result.Tables.FirstOrDefault((t) => t.Name.Equals(table.Name), null);
            if (resTable == null)
            {
                _logger.Error("The table doesn't exists!");
                throw new DataResourceException("The table doesn't exists!");
            }

            try
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.Remove(resTable);
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.Add(resTable);
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to drop table!");
            }
        }

        public List<string[]> GetDBData()
        {
            List<string[]> data = new List<string[]>();

            foreach (var db in _dataBases)
            {
                string[] dbData = new string[db.Tables.Count+1];
                dbData[0] = db.Name;
                for (int i = 0; i < db.Tables.Count; i++)
                {
                    dbData[i+1] = db.Tables[i].Name;
                }
                data.Add(dbData);
            }

            return data;
        }
    }
}
