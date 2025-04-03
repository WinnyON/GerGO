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

        // DATA DEFINITION
        public void AddColumn(string dbName, string tableName, Column column)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            try
            {
                _metaDataManager.AddColumn(dbName, tableName, column);
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
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
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsColumn(dbName, tableName, foreignKey.AttributeName))
            {
                _logger.Error($"Attribute {foreignKey.AttributeName} doesn't exist!");
                throw new DataResourceException($"Attribute {foreignKey.AttributeName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, foreignKey.RefTableName))
            {
                _logger.Error($"Referenced table {foreignKey.RefTableName} doesn't exist!");
                throw new DataResourceException($"Referenced table {foreignKey.RefTableName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsColumn(dbName, tableName, foreignKey.RefAttributeName))
            {
                _logger.Error($"Referenced attribute {foreignKey.RefAttributeName} doesn't exist!");
                throw new DataResourceException($"Referenced attribute {foreignKey.RefAttributeName} doesn't exist!");
            }

            try
            {
                _metaDataManager.AddForeignKey(dbName, tableName, foreignKey);
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void AddTable(string dbName, Table table)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (_metaDataManager.ExitsTable(dbName, table.Name))
            {
                _logger.Error($"Table {table.Name} already exists!");
                throw new DataResourceException($"Table {table.Name} already exists!");
            }

            try
            {
                _metaDataManager.AddTable(dbName, table);
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropDataBase(DataBase dataBase)
        {
            if (!_metaDataManager.ExitsDb(dataBase.Name))
            {
                _logger.Error($"Database {dataBase.Name} doesn't exist!");
                throw new DataResourceException($"Database {dataBase.Name} doesn't exist!");
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
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, table.Name))
            {
                _logger.Error($"Table {table.Name} doesn't exist!");
                throw new DataResourceException($"Table {table.Name} doesn't exist!");
            }

            try
            {
                _metaDataManager.DropTable(dbName, table);
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public string[] GetColumns(string dbName, string tableName)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            return _metaDataManager.GetColumns(dbName, tableName);
        }

        public List<string[]> GetDBData()
        {
            return _metaDataManager.GetDBData();
        }

        public List<string[]> GetForeignKeys(string dbName, string tableName)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            return _metaDataManager.GetForeignKeys(dbName, tableName);
        }

        public List<string[]> GetTableData(string dbName, string tableName)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExitsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            return _metaDataManager.GetTableData(dbName, tableName);
        }

        public string[] GetTables(string dbName)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            return _metaDataManager.GetTables(dbName);
        }


        // DATA MANIPULATION

        public int Insert(string dbName, string tableName, string value)
        {
            throw new NotImplementedException();
        }
        public int Delete(string dbName, string tableName, string value)
        {
            throw new NotImplementedException();
        }
    }
}
