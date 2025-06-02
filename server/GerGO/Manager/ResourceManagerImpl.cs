using GerGO.DataAcces;
using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;
using GerGO.Models;
using GerGO.Query;
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
        public void AddColumn(string dbName, string tableName, Column column, PrimaryKey? pKey, bool isUnique)
        {
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (_metaDataManager.ExistsColumn(dbName, tableName, column.Name))
            {
                _logger.Error($"Column {column.Name} already exists!");
                throw new DataResourceException($"Column {column.Name} already exists!");
            }

            if (pKey != null && pKey.IdentitySeed != 0 && (column.NotNull || column.DefaultVal != string.Empty || isUnique || column.Check != string.Empty))
            {
                _logger.Error("Not valid column: pk with identity can have no other constraints!");
                throw new DataResourceException("Not valid column: pk with identity can have no other constraints!");
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
                    _metaDataManager.AddColumn(dbName, tableName, column, pKey, isUnique);
                    string value = column.DefaultVal == string.Empty ? "null" : column.DefaultVal;
                    _storedDataManager.AddColumn(dbName, tableName, value);
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
            if (_metaDataManager.ExistsDb(dataBase.Name))
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
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (_metaDataManager.ExistsForeignKey(dbName, tableName, foreignKey.Name))
            {
                _logger.Error($"Foreign key {foreignKey.Name} already exists!");
                throw new DataResourceException($"Foreign key {foreignKey.Name} already exists!");
            }

            if (!_metaDataManager.ExistsColumn(dbName, tableName, foreignKey.AttributeName))
            {
                _logger.Error($"Attribute {foreignKey.AttributeName} doesn't exist!");
                throw new DataResourceException($"Attribute {foreignKey.AttributeName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, foreignKey.RefTableName))
            {
                _logger.Error($"Referenced table {foreignKey.RefTableName} doesn't exist!");
                throw new DataResourceException($"Referenced table {foreignKey.RefTableName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsColumn(dbName, foreignKey.RefTableName, foreignKey.RefAttributeName))
            {
                _logger.Error($"Referenced attribute {foreignKey.RefAttributeName} doesn't exist!");
                throw new DataResourceException($"Referenced attribute {foreignKey.RefAttributeName} doesn't exist!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    var attrList = _metaDataManager.GetPrimaryKeys(dbName, tableName);
                    attrList.Add(foreignKey.AttributeName);
                    List<MongoEntity> indexData = GetAllRows(dbName, tableName, attrList).Select(row =>
                    {
                        List<string> values = row.Split('^').ToList();
                        string fKeyVal = values[-1];
                        values.RemoveAt(values.Count - 1);
                        string pKey = string.Join('^', values);
                        return new MongoEntity(fKeyVal, pKey);
                    }).ToList();

                    _storedDataManager.InsertIndexData(dbName, $"{tableName}_{foreignKey.Name}", indexData);
                    _metaDataManager.AddForeignKey(dbName, tableName, foreignKey);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropColumn(string dbName, string tableName, string colName)
        {
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsColumn(dbName, tableName, colName))
            {
                _logger.Error($"Column {colName} doesn't exist!");
                throw new DataResourceException($"2^Column {colName} doesn't exist!");
            }

            if (_metaDataManager.HasFkConstraint(dbName, tableName, colName))
            {
                _logger.Error($"Column {colName} has foreign key constraint!");
                throw new DataResourceException($"Column {colName} has foreign key constraint!");
            }

            Table table = _metaDataManager.GetTable(dbName, tableName);
            if (table.PrimaryKeys.Any(pk => pk.Name.Equals(colName)))
            {
                _logger.Error("You can't remove a primary key!");
                throw new DataResourceException("You can't remove a primary key!");
            }

            if (table.IndexFiles.Any(iFile => iFile.Attributes.Contains(colName)))
            {
                _logger.Error("The column has an index on it!");
                throw new DataResourceException("The column has an index on it!");
            }

            Column col = _metaDataManager.GetColumn(dbName, tableName, colName);
            bool isUnique = table.UniqueKeys.Contains(colName);
            List<IndexFile> iFiles = table.IndexFiles.FindAll(iFile => iFile.Attributes.Contains(colName));
            try
            {
                lock (_locks[dbName])
                {
                    if (isUnique)
                    {
                        _storedDataManager.DropTable(dbName, $"{tableName}_{colName}_uniquekey");
                    }

                    // actual stored data update
                    int index = _metaDataManager.GetColumnPostions(dbName, tableName, [colName])[0] - table.PrimaryKeys.Count;

                    _storedDataManager.RemoveColumn(dbName, tableName, index);
                    _metaDataManager.DropColumn(dbName, tableName, col);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropForeignKey(string dbName, string tableName, string fkName)
        {
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsForeignKey(dbName, tableName, fkName))
            {
                _logger.Error($"Foreign key {fkName} doesn't exist!");
                throw new DataResourceException($"Foreign key {fkName} doesn't exist!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    ForeignKey fKey = _metaDataManager.GetForeignKey(dbName, tableName, fkName);
                    _metaDataManager.DropForeignKey(dbName, tableName, fKey);
                    _storedDataManager.DropTable(dbName, $"{tableName}_{fKey.Name}");
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void AddIndexFile(string dbName, string tableName, IndexFile iFile)
        {
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (_metaDataManager.ExistsIndex(dbName, tableName, iFile.Name))
            {
                _logger.Error($"Index {iFile.Name} alredy exists!");
                throw new DataResourceException($"Index {iFile.Name} alredy exists!");
            }

            foreach (var column in iFile.Attributes)
            {
                if (!_metaDataManager.ExistsColumn(dbName, tableName, column))
                {
                    _logger.Error($"Column {column} doesn't exist!");
                    throw new DataResourceException($"Column {column} doesn't exist!");
                }
            }

            foreach (var column in iFile.Attributes)
            {
                if (_metaDataManager.GetPrimaryKeys(dbName, tableName).Contains(column))
                {
                    _logger.Error("You can't create an index for a primary key!");
                    throw new DataResourceException("You can't create an index for a primary key!");
                }
            }

            try
            {
                lock (_locks[dbName])
                {
                    _metaDataManager.AddIndex(dbName, tableName, iFile);

                    var attrList = _metaDataManager.GetPrimaryKeys(dbName, tableName);
                    attrList.AddRange(iFile.Attributes);
                    int nrPKeys = attrList.Count - iFile.Attributes.Count;
                    List<MongoEntity> indexData = GetAllRows(dbName, tableName, attrList).Select(row =>
                    {
                        List<string> values = row.Split('^').ToList();
                        string indVal = string.Join('^', values.Skip(nrPKeys));
                        string pKey = string.Join('^', values.Take(nrPKeys));
                        return new MongoEntity(indVal, pKey);
                    }).ToList();

                    _storedDataManager.InsertIndexData(dbName, $"{tableName}_{iFile.Name}", indexData);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropIndex(string dbName, string tableName, string indexName)
        {
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
            {
                _logger.Error($"Table {tableName} doesn't exist!");
                throw new DataResourceException($"Table {tableName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsIndex(dbName, tableName, indexName))
            {
                _logger.Error($"Index {indexName} doesn't exist!");
                throw new DataResourceException($"Index {indexName} doesn't exist!");
            }

            IndexFile index = _metaDataManager.GetIndexFile(dbName, tableName, indexName);

            try
            {
                lock (_locks[dbName])
                {
                    _storedDataManager.DropTable(dbName, $"{tableName}_{index.Name}");
                    _metaDataManager.DropIndex(dbName, tableName, index);
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
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (_metaDataManager.ExistsTable(dbName, table.Name))
            {
                _logger.Error($"Table {table.Name} already exists!");
                throw new DataResourceException($"Table {table.Name} already exists!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    _metaDataManager.AddTable(dbName, table);
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
            if (!_metaDataManager.ExistsDb(dataBase.Name))
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
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, table.Name))
            {
                _logger.Error($"Table {table.Name} doesn't exist!");
                throw new DataResourceException($"Table {table.Name} doesn't exist!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    table.IndexFiles.ForEach(iFile => _storedDataManager.DropTable(dbName, $"{table.Name}_{iFile.Name}"));
                    table.UniqueKeys.ForEach(uKey => _storedDataManager.DropTable(dbName, $"{table.Name}_{uKey}_uniquekey"));
                    table.ForeignKeys.ForEach(fKey => _storedDataManager.DropTable(dbName, $"{table.Name}_{fKey.Name}"));
                    _storedDataManager.DropTable(dbName, table.Name);
                    _metaDataManager.DropTable(dbName, table);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }


        public List<string[]> GetDBData()
        {
            return _metaDataManager.GetDBData();
        }

        public List<string[]> GetForeignKeys(string dbName, string tableName)
        {
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
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
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
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
            if (!_metaDataManager.ExistsDb(dbName))
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
            if (!_metaDataManager.ExistsDb(dbName))
            {
                _logger.Error($"Database {dbName} doesn't exist!");
                throw new DataResourceException($"Database {dbName} doesn't exist!");
            }

            if (!_metaDataManager.ExistsTable(dbName, tableName))
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

        public int Insert(string dbName, string tableName, List<string> columnNames, List<string> rows)
        {
            if (!_metaDataManager.ExistsDb(dbName) || !_metaDataManager.ExistsTable(dbName, tableName))
            {
                throw new DataResourceException("Table doesn't exist");
            }

            foreach (var col in columnNames)
            {
                if (!_metaDataManager.ExistsColumn(dbName, tableName, col))
                    throw new DataResourceException("Column doesn't exists!");
            }

            Table table = _metaDataManager.GetTable(dbName, tableName);
            int count = 0;
            try
            {
                lock (_locks[dbName])
                {
                    List<MongoEntity> insertData = [];
                    Dictionary<string, List<MongoEntity>> uniqueInsertData = [];
                    Dictionary<string, List<MongoEntity>> indexData = [];
                    Dictionary<string, List<MongoEntity>> fKeyData = [];
                    foreach (string row in rows)
                    {
                        try
                        {
                            // validate pk and attributes
                            string pKey = _metaDataManager.GetPrimaryKey(dbName, table, row, columnNames);
                            string value = _metaDataManager.GetValuePart(table, row, columnNames);

                            string[] rowSplitted = row.Split('^');
                            // foreign key check
                            foreach (var fk in table.ForeignKeys)
                            {
                                string fKeyVal = rowSplitted[columnNames.IndexOf(fk.AttributeName)];
                                if (!_storedDataManager.ExistsKey(dbName, fk.RefTableName, fKeyVal))
                                    throw new DataAccesException("");

                                bool isUnique = _metaDataManager.GetTable(dbName, fk.RefTableName).UniqueKeys.Contains(fk.RefAttributeName);
                                if (isUnique && !_storedDataManager.ExistsKey(dbName, $"{fk.RefAttributeName}_{fk.RefAttributeName}_uniquekey", fKeyVal))
                                    throw new DataAccesException("");
                            }

                            // unique key check
                            foreach (var uKey in table.UniqueKeys)
                            {
                                string uniqueVal = rowSplitted[columnNames.IndexOf(uKey)];
                                if (_storedDataManager.ExistsKey(dbName, $"{tableName}_{uKey}_uniquekey", uniqueVal))
                                    throw new DataAccesException("");
                                string tmp = $"{tableName}_{uKey}_uniquekey";
                                if (uniqueInsertData.ContainsKey(tmp))
                                    uniqueInsertData[tmp].Add(new MongoEntity(uniqueVal, pKey));
                                else
                                    uniqueInsertData.Add(tmp, [new MongoEntity(uniqueVal, pKey)]);
                            }

                            foreach (var fk in table.ForeignKeys)
                            {
                                string fKeyVal = rowSplitted[columnNames.IndexOf(fk.AttributeName)];
                                if (fKeyData.ContainsKey(fk.Name))
                                    fKeyData[fk.Name].Add(new MongoEntity(fKeyVal, pKey));
                                else
                                    fKeyData.Add(fk.Name, [new MongoEntity(fKeyVal, pKey)]);
                            }

                            // inserting data
                            insertData.Add(new MongoEntity(pKey, value));

                            // inserting to index files
                            foreach (var iFile in table.IndexFiles)
                            {
                                string indexKey = rowSplitted[columnNames.IndexOf(iFile.Attributes[0])];
                                for (int i = 1; i < iFile.Attributes.Count; i++)
                                {
                                    indexKey = indexKey + "^" + rowSplitted[columnNames.IndexOf(iFile.Attributes[i])];
                                }
                                if (indexData.ContainsKey(iFile.Name))
                                    fKeyData[iFile.Name].Add(new MongoEntity(indexKey, pKey));
                                else
                                    fKeyData.Add(iFile.Name, [new MongoEntity(indexKey, pKey)]);
                            }

                            count++;
                        }
                        catch (DataAccesException)
                        {
                            continue;
                        }

                    }
                    if (insertData.Count == 0)
                        return 0;

                    // batched inserts
                    _storedDataManager.Insert(dbName, tableName, insertData);
                    foreach (var iFileData in indexData)
                    {
                        _storedDataManager.InsertIndexData(dbName, $"{tableName}_{iFileData.Key}", iFileData.Value);
                    }
                    foreach (var fKData in fKeyData)
                    {
                        _storedDataManager.InsertIndexData(dbName, $"{tableName}_{fKData.Key}", fKData.Value);
                    }
                    foreach (var uniqueData in uniqueInsertData)
                    {
                        _storedDataManager.Insert(dbName, uniqueData.Key, uniqueData.Value);
                    }
                }
                return count;
            }
            catch (Exception)
            {
                throw new DataResourceException("");
            }
        }
        public int Delete(string dbName, string tableName, List<string> keys)
        {
            if (!_metaDataManager.ExistsDb(dbName) || !_metaDataManager.ExistsTable(dbName, tableName) || keys.Count == 0)
            {
                throw new DataResourceException("Table doesn't exist");
            }

            Table table = _metaDataManager.GetTable(dbName, tableName);
            int count = 0;
            lock (_locks[dbName])
            {
                List<string> pKeysToDelete = [];
                foreach (var key in keys)
                {
                    try
                    {
                        // foreign keys that refer to this table
                        List<string> refForeignKeys = _metaDataManager.GetReferingForeignKeys(dbName, tableName);
                        refForeignKeys.ForEach(fk =>
                        {
                            if (_storedDataManager.ExistsKey(dbName, fk, key))
                                throw new DataAccesException("");
                        });

                        count++;
                        pKeysToDelete.Add(key);
                    }
                    catch (DataAccesException)
                    {
                        continue;
                    }
                }
                if (pKeysToDelete.Count == 0)
                    return 0;

                _storedDataManager.Delete(dbName, tableName, pKeysToDelete);
                foreach (var iFile in table.IndexFiles)
                {
                    _storedDataManager.DeleteIndexData(dbName, $"{tableName}_{iFile.Name}", pKeysToDelete);
                }
                foreach (var fK in table.ForeignKeys)
                {
                    _storedDataManager.DeleteIndexData(dbName, $"{tableName}_{fK.Name}", pKeysToDelete);
                }
                foreach (var uKey in table.UniqueKeys)
                {
                    _storedDataManager.DeleteUniqueIndexData(dbName, $"{tableName}_{uKey}_uniquekey", pKeysToDelete);
                }

                return count;
            }
        }

        // DATA QUERY

        public List<string> GetAllRows(string dbName, string tableName, List<string> columnNames)
        {
            if (!_metaDataManager.ExistsDb(dbName))
                throw new DataResourceException("Db doesn't exists!");

            if (!_metaDataManager.ExistsTable(dbName, tableName))
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
                    rows = _storedDataManager.GetAllRows(dbName, tableName);
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

            return resultSet;
        }

        public List<string> Select(SelectData selectData, ref string columnNames)
        {
            try
            {
                List<string> rows;
                lock (_locks[selectData.DbName])
                {
                    IQueryExecuter queryExecuter = QueryExecuterFactory.GetExecuter(_metaDataManager, _storedDataManager);
                    rows = queryExecuter.ExecuteQuery(ref selectData);
                }

                return rows;
            }
            catch (QueryExecuterException ex)
            {
                _logger.Error($"Failed to execute select query: {ex.Message}");
                throw new DataResourceException($"Failed to execute select query: {ex.Message}");
            }
        }

        public int DeleteWhere(string dbName, string tableName, List<string[]> wheres)
        {
            SelectData selectData = new SelectData();
            selectData.DbName = dbName;
            selectData.TableName = tableName;
            selectData.WhereClauses = wheres.Select(where =>
            {
                var tmp = where.ToList();
                tmp.Insert(0, tableName);
                tmp.Insert(0, "21");
                return tmp.ToArray();
            }).ToList();
            _metaDataManager.GetTable(dbName, tableName).PrimaryKeys.ForEach(fk => selectData.Columns.Add(["25", tableName, fk.Name]));

            List<string> keys;
            try
            {
                IQueryExecuter queryExecuter = QueryExecuterFactory.GetExecuter(_metaDataManager, _storedDataManager);
                keys = queryExecuter.ExecuteQuery(ref selectData);
                int count = Delete(dbName, tableName, keys);

                return count;
            }
            catch (QueryExecuterException ex)
            {
                _logger.Error($"Failed to execute delete where query: {ex.Message}");
                throw new DataResourceException($"Failed to execute delete where query: {ex.Message}");
            }
        }
    }
}
