using GerGO.DataAcces;
using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;
using GerGO.Models;
using GerGO.Utils;

namespace GerGO.Manager
{
    class ResourceManagerImpl : IResourceManager
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        private IMetaDataManager _metaDataManager = MetaDataManagerFactory.GetMetaDataManager();
        private IStoredDataManager _storedDataManager = StoredDataManagerFactory.GetStoredDataManager();

        public ResourceManagerImpl()
        {
        }

        public void AddColumn(string dbName, string tableName, Column column)
        {
            throw new NotImplementedException();
        }

        public void AddDataBase(DataBase dataBase)
        {
            if (_metaDataManager.ExitsDb(dataBase.Name))
            {
                _logger.Error($"Database {dataBase.Name} already exists!");
                throw new DataResourceException($"Database {dataBase.Name} already exists!");
            }

            try
            {
                _metaDataManager.AddDatabase(dataBase);
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey)
        {
            throw new NotImplementedException();
        }

        public void AddTable(string dbName, Table table)
        {
            throw new NotImplementedException();
        }

        public int Delete(string dbName, string tableName, string value)
        {
            throw new NotImplementedException();
        }

        public void DropDataBase(DataBase dataBase)
        {
            if (!_metaDataManager.ExitsDb(dataBase.Name))
            {
                _logger.Error($"Database {dataBase.Name} doesn't exist!");
                throw new DataResourceException($"Database {dataBase.Name} doesn't exists!");
            }

            try
            {
                _metaDataManager.DropDatabase(dataBase);
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropTable(string dbName, Table table)
        {
            throw new NotImplementedException();
        }

        public List<string[]> GetColumns(string dbName, string tableName)
        {
            throw new NotImplementedException();
        }

        public List<string[]> GetDBData()
        {
            throw new NotImplementedException();
        }

        public List<string[]> GetForeignKeys(string dbName, string tableName)
        {
            throw new NotImplementedException();
        }

        public List<string[]> GetTableData(string dbName, string tableName)
        {
            throw new NotImplementedException();
        }

        public string[] GetTables(string dbName)
        {
            throw new NotImplementedException();
        }

        public int Insert(string dbName, string tableName, string value)
        {
            throw new NotImplementedException();
        }
    }
}
