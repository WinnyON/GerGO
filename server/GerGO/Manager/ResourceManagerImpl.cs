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

            if (_metaDataManager.ExistsColumn(dbName, tableName, column.Name))
            {
                _logger.Error($"Column {column.Name} already exists!");
                throw new DataResourceException($"Column {column.Name} already exists!");
            }

            if (column.PKIdentity.Seed != 0 && (column.NotNull || column.DefaultVal != string.Empty || column.Unique || column.Check != string.Empty))
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
                    string mongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
                    _metaDataManager.AddColumn(dbName, tableName, column);
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

            if (!_metaDataManager.ExitsTable(dbName, foreignKey.RefTableName))
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
                    _metaDataManager.AddForeignKey(dbName, tableName, foreignKey);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropColumn(string dbName, string tableName, Column column)
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

            if (!_metaDataManager.ExistsColumn(dbName, tableName, column.Name))
            {
                _logger.Error($"Column {column.Name} doesn't exist!");
                throw new DataResourceException($"2^Column {column.Name} doesn't exist!");
            }
            Column col = _metaDataManager.GetColumn(dbName, tableName, column.Name);

            if (_metaDataManager.HasFkConstraint(dbName, tableName, column.Name))
            {
                _logger.Error($"Column {column.Name} has foreign key constraint!");
                throw new DataResourceException($"Column {column.Name} has foreign key constraint!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    string mongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
                    int index = _metaDataManager.GetColumnPostions(dbName, tableName, [col.Name])[0];
                    int nrPKeys = _metaDataManager.GetNrPkeys(dbName, tableName);
                    _metaDataManager.DropColumn(dbName, tableName, col);
                    _storedDataManager.RemoveColumn(dbName, mongoId, col.PrimaryKey, index, nrPKeys);
                }
            }
            catch (DataAccesException ex)
            {
                _logger.Error(ex.Message);
                throw new DataResourceException(ex.Message);
            }
        }

        public void DropForeignKey(string dbName, string tableName, ForeignKey foreignKey)
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

            if (!_metaDataManager.ExistsForeignKey(dbName, tableName, foreignKey.Name))
            {
                _logger.Error($"Foreign key {foreignKey.Name} doesn't exist!");
                throw new DataResourceException($"Foreign key {foreignKey.Name} doesn't exist!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    _metaDataManager.DropForeignKey(dbName, tableName, foreignKey);
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

            if (_metaDataManager.GetColumn(dbName, tableName, columnName).PrimaryKey)
            {
                _logger.Error("You can't create an index for a primary key!");
                throw new DataResourceException("You can't create an index for a primary key!");
            }

            if (_metaDataManager.ExistsIndex(dbName, tableName, indexName))
            {
                _logger.Error($"Index {indexName} alredy exists!");
                throw new DataResourceException($"Index {indexName} alredy exists!");
            }

            try
            {
                lock (_locks[dbName])
                {
                    string mongoID = _storedDataManager.AddIndexFile(dbName, tableName);
                    _metaDataManager.AddIndex(dbName, tableName, indexName, columnName, mongoID);

                    var colList = _metaDataManager.GetPrimaryKeys(dbName, tableName);
                    colList.Insert(0, columnName);
                    GetAllRows(dbName, tableName, colList).ForEach(row =>
                    {
                        List<string> values = row.Split('^').ToList();
                        string indexVal = values[0];
                        values.RemoveAt(0);
                        _storedDataManager.InsertToIndexFile(dbName, tableName, mongoID, string.Join('^', values), indexVal);
                    });
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
                    _storedDataManager.DropIndexFile($"{dbName}_{tableName}_indexfiles", index.MongoID);
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
            Dictionary<string, string> indexFiles = [];
            table.IndexFiles.ForEach(iFile => indexFiles.Add(iFile.Attributes[0], iFile.MongoID));
            try
            {
                lock (_locks[dbName])
                {
                    string key = "";
                    int innerSeed = 0;
                    if (!_storedDataManager.IsValidRow(dbName, table, columnNames, _metaDataManager.GetColumnPostions(dbName,tableName, columnNames), ref key, ref value, ref innerSeed))
                        throw new DataResourceException("");

                    // foreign key check
                    foreach (var fk in table.ForeignKeys)
                    {
                        if (columnNames.Contains(fk.AttributeName))
                        {
                            var refTable = _metaDataManager.GetTable(dbName, fk.RefTableName);
                            int indexRef = _metaDataManager.GetColumnPostions(dbName, fk.RefTableName, [fk.RefAttributeName])[0];
                            int index = _metaDataManager.GetColumnPostions(dbName, tableName, [fk.AttributeName])[0];
                            string insertedValue = (key + "^" + value).Split('^')[index];
                            if (!_storedDataManager.ContainsValue(dbName, refTable.MongoID, indexRef, insertedValue))
                            {
                                throw new DataResourceException("Error with foreign keys!");
                            }
                        }
                    }

                    foreach (var iFile in table.IndexFiles)
                    {
                        string[] insertedValues = value.Split('^');
                        int nrPKeys = _metaDataManager.GetNrPkeys(dbName, tableName);
                        string val = insertedValues[_metaDataManager.GetColumnPostions(dbName, tableName, [iFile.Attributes[0]])[0] - nrPKeys];
                        _storedDataManager.InsertToIndexFile(dbName, tableName, iFile.MongoID, key, val);
                    }

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
            catch (Exception)
            {
                throw new DataResourceException("");
            }
        }
        public void Delete(string dbName, string tableName, string key)
        {
            if (!_metaDataManager.ExitsDb(dbName) || !_metaDataManager.ExitsTable(dbName, tableName) || string.IsNullOrEmpty(key))
            {
                throw new DataResourceException("Table doesn't exist");
            }

            string tableMongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
            Table table = _metaDataManager.GetTable(dbName, tableName);
            try
            {
                lock (_locks[dbName])
                {
                    string row = key + "^" + _storedDataManager.GetValue(dbName, tableMongoId, key);
                    // foreign key check
                    List<Table> tables = _metaDataManager.GetTables(dbName).ToList().Select(t => _metaDataManager.GetTable(dbName, t)).ToList();
                    foreach (var t in tables)
                    {
                        List<ForeignKey> fKeys = t.ForeignKeys.FindAll(fk => fk.RefTableName.Equals(tableName));

                        foreach (var fk in fKeys)
                        {
                            string colValue = row.Split('^')[_metaDataManager.GetColumnPostions(dbName, tableName, [fk.RefAttributeName])[0]];
                            int index = _metaDataManager.GetColumnPostions(dbName, t.Name, [fk.AttributeName])[0];
                            if (_storedDataManager.ContainsValue(dbName, t.MongoID, index, colValue))
                                throw new DataResourceException("Value referrenced by foreign key!");
                        }
                    }

                    _storedDataManager.Delete(dbName, tableMongoId, key);
                    foreach (var indexFile in table.IndexFiles)
                    {
                        _storedDataManager.DeleteFromIndexFile(dbName, tableName, indexFile.MongoID, key);
                    }
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

            return resultSet;
        }

        public List<string> Select(SelectData selectData, ref string columnNames)
        {
            if (!IsValidSelectData(ref selectData, ref columnNames))
            {
                throw new DataResourceException("Not valid select data!");
            }

            List<string> rows = [];
            try
            {
                lock (_locks[selectData.DbName])
                {
                    rows = JoinTables(selectData);
                }

                ExecuteWhereCluases(selectData, ref rows);
                ExecuteProjection(selectData, ref rows);
            }
            catch (DataAccesException ex)
            {
                _logger.Error($"Error at select: {ex.Message}");
                throw new DataResourceException($"Error at select: {ex.Message}");
            }

            return rows;
        }

        private void ExecuteWhereCluases(SelectData selectData, ref List<string> rows)
        {
            List<string[]> indexedWheres = [];
            List<IndexFile> indexes = [];
            List<string[]> notIndexedWheres = [];
            foreach (var where in selectData.WhereClauses)
            {
                string? indName = _metaDataManager.HasIndexOnIt(selectData.DbName, selectData.TableName, where[2]);
                if (indName != null)
                {
                    indexedWheres.Add(where);
                    indexes.Add(_metaDataManager.GetIndexFile(selectData.DbName, selectData.TableName, indName));
                }
                else
                {
                    notIndexedWheres.Add(where);
                }
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                var table = _metaDataManager.GetTable(selectData.DbName, indexedWheres[i][1]);
                var column = _metaDataManager.GetColumn(selectData.DbName, indexedWheres[i][1], indexedWheres[i][2]);
                List<string> keys = _storedDataManager.GetValuesWhere($"{selectData.DbName}_{selectData.TableName}_indexfiles", indexes[i].MongoID, column.Type,
                    indexedWheres[i][3], indexedWheres[i][4]);
                List<string> values = _storedDataManager.GetValues(selectData.DbName, table.MongoID, keys);
                rows = rows.Intersect(values).ToList();
            }

            rows = rows.FindAll(row =>
            {
                bool ok = true;
                foreach (var where in notIndexedWheres)
                {
                    Column col = _metaDataManager.GetColumn(selectData.DbName, selectData.TableName, where[2]);
                    string[] rowData = row.Split('^');
                    int pos = _metaDataManager.GetColumnPostions(selectData.DbName, selectData.TableName, [where[2]])[0];
                    switch (where[3])
                    {
                        case "=":
                        case "==":
                            if (!Validator.IsEqual(rowData[pos], where[4], col.Type))
                                ok = false;
                            break;
                        case ">":
                            if (!Validator.IsGreater(rowData[pos], where[4], col.Type))
                                ok = false;
                            break;
                        case ">=":
                            if (!Validator.IsGreaterOrEqual(rowData[pos], where[4], col.Type))
                                ok = false;
                            break;
                        case "<":
                            if (!Validator.IsLess(rowData[pos], where[4], col.Type))
                                ok = false;
                            break;
                        case "<=":
                            if (!Validator.IsLessOrEqual(rowData[pos], where[4], col.Type))
                                ok = false;
                            break;
                        default:
                            break;
                    }
                }
                return ok;
            });
        }

        private void ExecuteProjection(SelectData selectData, ref List<string> rows)
        {
            List<string> columnNames = selectData.Columns.Select(col => col[2]).ToList();
            List<int> positions = _metaDataManager.GetColumnPostions(selectData.DbName, selectData.TableName, columnNames);
            rows = rows.Select(r =>
            {
                string[] values = r.Split('^');
                string tmp = values[positions[0]];
                for (int i = 1; i < positions.Count; i++)
                {
                    tmp = tmp + "^" + values[positions[i]];
                }
                return tmp;
            }).ToList();
        }

        private List<string> JoinTables(SelectData selectData)
        {
            return _storedDataManager.GetAllRows(selectData.DbName, _metaDataManager.GetTableMongoId(selectData.DbName, selectData.TableName));
        }

        private bool IsValidSelectData(ref SelectData selectData, ref string columnNames)
        {
            if (!_metaDataManager.ExitsDb(selectData.DbName) || !_metaDataManager.ExitsTable(selectData.DbName, selectData.TableName))
                return false;

            string tableName = selectData.TableName;
            columnNames = string.Empty;
            if (selectData.Columns.Count == 1 && selectData.Columns[0][1] == "*")
            {
                var colNames = _metaDataManager.GetColumns(selectData.DbName, selectData.TableName);
                selectData.Columns.Clear();
                selectData.Columns = colNames.Select(col => new string[] { "25", tableName, col }).ToList();
                string tmp = string.Empty;
                selectData.Columns.ForEach(col => tmp = $"{tmp}^{tableName}.{col[2]}");
                columnNames = tmp.Substring(1, tmp.Length - 1);
            }
            else
            {
                foreach (var col in selectData.Columns)
                {
                    if (col.Length != 3)
                        return false;
                    if (!_metaDataManager.ExitsTable(selectData.DbName, col[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, col[1], col[2]))
                        return false;
                }
            }

            foreach (var jTable in selectData.JoinTables)
            {
                if (jTable.Length != 5)
                    return false;
                if (!_metaDataManager.ExitsTable(selectData.DbName, jTable[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, jTable[1], jTable[2]))
                    return false;
                if (!_metaDataManager.ExitsTable(selectData.DbName, jTable[3]) || !_metaDataManager.ExistsColumn(selectData.DbName, jTable[4], jTable[2]))
                    return false;
            }

            List<string> operators = ["<", ">", "=", "<=", ">=", "<>"];
            foreach (var where in selectData.WhereClauses)
            {
                if (where.Length != 5)
                    return false;
                if (!_metaDataManager.ExitsTable(selectData.DbName, where[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, where[1], where[2]))
                    return false;
                if (!operators.Contains(where[3]))
                    return false;
            }

            foreach (var groupBy in  selectData.GroupByClauses)
            {
                if (groupBy.Length != 3)
                    return false;
                if (!_metaDataManager.ExitsTable(selectData.DbName, groupBy[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, groupBy[1], groupBy[2]))
                    return false;
            }

            foreach (var having in selectData.HavingClauses)
            {
                if (having.Length != 5)
                    return false;
                if (!_metaDataManager.ExitsTable(selectData.DbName, having[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, having[1], having[2]))
                    return false;
                if (!operators.Contains(having[3]))
                    return false;
            }

            foreach (var orderBy in  selectData.OrderByCluases)
            {
                if (orderBy.Length != 3)
                    return false;
                if (!_metaDataManager.ExitsTable(selectData.DbName, orderBy[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, orderBy[1], orderBy[2]))
                    return false;
            }

            return true;
        }

        public void DeleteWhere(string dbName, string tableName, List<string[]> wheres)
        {
            if (!_metaDataManager.ExitsDb(dbName) || !_metaDataManager.ExitsTable(dbName, tableName))
            {
                throw new DataResourceException("Not valid delete data!");
            }

            List<string> operators = ["<", ">", "=", "<=", ">=", "<>"];
            foreach (var where in wheres)
            {
                if (!_metaDataManager.ExistsColumn(dbName, tableName, where[0]) || !operators.Contains(where[1]))
                {
                    throw new DataResourceException("Not valid delete data!");
                }
            }

            List<string[]> indexedWheres = [];
            List<IndexFile> indexes = [];
            List<string[]> notIndexedWheres = [];
            foreach (var where in wheres)
            {
                string? indName = _metaDataManager.HasIndexOnIt(dbName, tableName, where[0]);
                if (indName != null)
                {
                    indexedWheres.Add(where);
                    indexes.Add(_metaDataManager.GetIndexFile(dbName, tableName, indName));
                }
                else
                {
                    notIndexedWheres.Add(where);
                }
            }

            List<string> rows = _storedDataManager.GetAllRows(dbName, _metaDataManager.GetTableMongoId(dbName, tableName));

            for (int i = 0; i < indexes.Count; i++)
            {
                var table = _metaDataManager.GetTable(dbName, tableName);
                var column = _metaDataManager.GetColumn(dbName, tableName, indexedWheres[i][0]);
                List<string> keys = _storedDataManager.GetValuesWhere($"{dbName}_{tableName}_indexfiles", indexes[i].MongoID, column.Type,
                    indexedWheres[i][1], indexedWheres[i][2]);
                List<string> values = _storedDataManager.GetValues(dbName, table.MongoID, keys);
                rows = rows.Intersect(values).ToList();
            }

            List<string> keysToDelete= rows.FindAll(row =>
            {
                bool ok = true;
                foreach (var where in notIndexedWheres)
                {
                    Column col = _metaDataManager.GetColumn(dbName, tableName, where[0]);
                    string[] rowData = row.Split('^');
                    int pos = _metaDataManager.GetColumnPostions(dbName, tableName, [where[0]])[0];
                    switch (where[1])
                    {
                        case "=":
                        case "==":
                            if (!Validator.IsEqual(rowData[pos], where[2], col.Type))
                                ok = false;
                            break;
                        case ">":
                            if (!Validator.IsGreater(rowData[pos], where[2], col.Type))
                                ok = false;
                            break;
                        case ">=":
                            if (!Validator.IsGreaterOrEqual(rowData[pos], where[2], col.Type))
                                ok = false;
                            break;
                        case "<":
                            if (!Validator.IsLess(rowData[pos], where[2], col.Type))
                                ok = false;
                            break;
                        case "<=":
                            if (!Validator.IsLessOrEqual(rowData[pos], where[2], col.Type))
                                ok = false;
                            break;
                        default:
                            break;
                    }
                }
                return ok;
            }).Select(row => row.Split('^')[0]).ToList();

            keysToDelete.ForEach(key =>
            {
                string tableMongoId = _metaDataManager.GetTableMongoId(dbName, tableName);
                Table table = _metaDataManager.GetTable(dbName, tableName);
                try
                {
                    lock (_locks[dbName])
                    {
                        string row = key + "^" + _storedDataManager.GetValue(dbName, tableMongoId, key);
                        // foreign key check
                        List<Table> tables = _metaDataManager.GetTables(dbName).ToList().Select(t => _metaDataManager.GetTable(dbName, t)).ToList();
                        foreach (var t in tables)
                        {
                            List<ForeignKey> fKeys = t.ForeignKeys.FindAll(fk => fk.RefTableName.Equals(tableName));

                            foreach (var fk in fKeys)
                            {
                                string colValue = row.Split('^')[_metaDataManager.GetColumnPostions(dbName, tableName, [fk.RefAttributeName])[0]];
                                int index = _metaDataManager.GetColumnPostions(dbName, t.Name, [fk.AttributeName])[0];
                                if (_storedDataManager.ContainsValue(dbName, t.MongoID, index, colValue))
                                    throw new DataResourceException("Value referrenced by foreign key!");
                            }
                        }

                        _storedDataManager.Delete(dbName, tableMongoId, key);
                        foreach (var indexFile in table.IndexFiles)
                        {
                            _storedDataManager.DeleteFromIndexFile(dbName, tableName, indexFile.MongoID, key);
                        }
                    }
                }
                catch (DataAccesException ex)
                {
                    _logger.Error($"Failed to delete: {ex.Message}");
                    throw new DataResourceException(ex.Message);
                }
            });
        }
    }
}
