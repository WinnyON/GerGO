using GerGO.DataAcces;
using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;
using GerGO.Models;
using GerGO.Utils;

namespace GerGO.Manager
{
    class ResourceManagerImpl : IResourceManager
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        private readonly IMetaDataManager _metaDataManager = MetaDataManagerFactory.GetMetaDataManager();
        private readonly IStoredDataManager _storedDataManager = StoredDataManagerFactory.GetStoredDataManager();
        private Dictionary<string, object> _locks;
        public ResourceManagerImpl()
        {
            LoadLocks();
        }

        private void LoadLocks()
        {
            List<string[]> dbData = _metaDataManager.GetDBData();
            _locks = new Dictionary<string, object>();
            foreach (var db in dbData)
            {
                _locks[db[0]] = new object();
            }
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

            if (column.PrimaryKey && (column.NotNull || column.DefaultVal != string.Empty || column.Unique || column.Check != string.Empty))
            {
                _logger.Error("Not valid column: pk key can have no other constraints!");
                throw new DataResourceException("Not valid column: pk key can have no other constraints!");
            }

            if (column.NotNull && column.DefaultVal == string.Empty)
            {
                _logger.Error("When set NOT NULL, default value is required!");
                throw new DataResourceException("When set NOT NULL, default value is required!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    _metaDataManager.AddColumn(dbName, tableName, column);
                    string mongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
                    string value = column.DefaultVal == string.Empty ? "null" : column.DefaultVal;
                    _storedDataManager.AddColumn(dbName, mongoId, value);
                }
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
                _locks[dataBase.Name] = new object();
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
                lock (_locks[dbName])
                {
                    _metaDataManager.AddForeignKey(dbName, tableName, foreignKey);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void AddIndexFile(string dbName, string tableName, string indexName, string columnName)
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

            if (!_metaDataManager.ExistsColumn(dbName, tableName, columnName))
            {
                _logger.Error($"Column {columnName} doesn't exist!");
                throw new DataResourceException($"Column {columnName} doesn't exist!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    _metaDataManager.AddIndex(dbName, tableName, indexName, columnName);
                }
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
                lock (_locks[dbName])
                {
                    string mongoId = _storedDataManager.PrepareTable(dbName, table.Name);
                    _metaDataManager.AddTable(dbName, mongoId, table);
                }
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
                lock (_locks[dataBase.Name])
                {
                    _metaDataManager.DropDatabase(dataBase);
                    _storedDataManager.DropDatabase(dataBase.Name);
                }
                _locks.Remove(dataBase.Name);
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
                lock (_locks[dbName])
                {
                    _storedDataManager.DropTable(dbName, _metaDataManager.GetTableMongoId(dbName, table.Name));
                    _metaDataManager.DropTable(dbName, table);
                }
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

            string[] columns;
            lock (_locks[dbName])
            {
                columns = _metaDataManager.GetColumns(dbName, tableName);
            }
            return columns;
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

            List<string[]> fKeys;
            lock (_locks[dbName])
            {
                fKeys = _metaDataManager.GetForeignKeys(dbName, tableName);
            }
            return fKeys;
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

            List<string[]> tableData;
            lock (_locks[dbName])
            {
                tableData = _metaDataManager.GetTableData(dbName, tableName);
            }
            return tableData;
        }

        public string[] GetTables(string dbName)
        {
            if (!_metaDataManager.ExitsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            string[] tables;
            lock (_locks[dbName])
            {
                tables = _metaDataManager.GetTables(dbName);
            }
            return tables;
        }

        public List<string> GetIndexes(string dbName, string tableName)
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

            List<string> indexData;
            lock (_locks[dbName])
            {
                indexData = _metaDataManager.GetIndexData(dbName, tableName);
            }
            return indexData;
        }

        // DATA MANIPULATION

        public void Insert(string dbName, string tableName, List<string> columnNames, string value)
        {
            if (!_metaDataManager.ExitsDb(dbName) || !_metaDataManager.ExitsTable(dbName, tableName))
            {
                throw new DataResourceException("Table doesn't exist");
            }

            foreach (var col in columnNames)
            {
                if (!_metaDataManager.ExistsColumn(dbName, tableName, col))
                    throw new DataResourceException("Column doesn't exists!");
            }

            Table table = _metaDataManager.GetTable(dbName, tableName);
            try
            {
                lock (_locks[dbName])
                {
                    string key = "";
                    int innerSeed = 0;
                    if (!_storedDataManager.IsValidRow(table, columnNames, ref key, ref value, ref innerSeed))
                        throw new DataResourceException("");
                    
                    _storedDataManager.Insert(dbName, table.MongoID, key, value);
                    if (innerSeed > 0)
                        _metaDataManager.UpdateInnerSeed(dbName,tableName, innerSeed);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error($"Failed to insert: {ex.Message}");
                throw new DataResourceException(ex.Message);
            }
        }
        public void Delete(string dbName, string tableName, string key)
        {
            if (!_metaDataManager.ExitsDb(dbName) || !_metaDataManager.ExitsTable(dbName, tableName))
            {
                throw new DataResourceException("Table doesn't exist");
            }

            string tableMongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
            try
            {
                lock (_locks[dbName])
                {
                    _storedDataManager.Delete(dbName, tableMongoId, key);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error($"Failed to delete: {ex.Message}");
                throw new DataResourceException(ex.Message);
            }
        }

        // DATA QUERY

        public List<string> GetAllRows(string dbName, string tableName, List<string> columnNames)
        {
            if (!_metaDataManager.ExitsDb(dbName))
                throw new DataResourceException("Db doesn't exists!");

            if (!_metaDataManager.ExitsTable(dbName, tableName))
                throw new DataResourceException("Table doesn't exists!");

            foreach (var col in columnNames)
            {
                if (!_metaDataManager.ExistsColumn(dbName, tableName, col))
                    throw new DataResourceException("Column doesn't exists!");
            }

            List<string> rows;
            try
            {
                lock (_locks[dbName])
                {
                    string mongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
                    rows = _storedDataManager.GetAllRows(dbName, mongoId);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error($"Failed to retrieve all rows: {ex.Message}");
                throw new DataResourceException("Failed to retrieve all rows!");
            }

            List<int> positions = _metaDataManager.GetColumnPostions(dbName, tableName, columnNames);
            List<string> resultSet = rows.Select(r =>
            {
                string[] values = r.Split('^');
                string tmp = values[positions[0]];
                for (int i = 1;i<positions.Count;i++)
                {
                    tmp = tmp + "^" + values[positions[i]];
                }
                return tmp;
            }).ToList();

            return rows;
        }
    }
}
