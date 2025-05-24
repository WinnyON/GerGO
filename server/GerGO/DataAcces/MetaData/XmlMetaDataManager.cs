using GerGO.DataAcces.MetaData.FileHandler;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;

namespace GerGO.DataAcces.MetaData
{
    class XmlMetaDataManager : IMetaDataManager
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();

        private readonly List<DataBase> _dataBases;
        private readonly IFileHandler _fileHandler = FileHandlerFactory.GetHandler();
        private readonly string _dbDataSourceFile;

        public XmlMetaDataManager()
        {
            _dbDataSourceFile = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Catalog.xml"));
            _dataBases = _fileHandler.ReadDataBaseData(_dbDataSourceFile);
        }

        public void AddColumn(string dbName, string tableName, Column column, PrimaryKey? pKey, bool isUnique)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Add(column);
            if (pKey != null)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).PrimaryKeys.Add(pKey);
            }
            if (isUnique)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).UniqueKeys.Add(column.Name);
            }
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(column);
                if (pKey != null)
                {
                    _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).PrimaryKeys.Remove(pKey);
                }
                if (isUnique)
                {
                    _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).UniqueKeys.Remove(column.Name);
                }
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to add column!");
            }
        }

        public void AddDatabase(DataBase database)
        {
            _dataBases.Add(database);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.Remove(database);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to create db!");
            }
        }

        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey)
        {
            ForeignKey? fk = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.FirstOrDefault(fKey => fKey.Name.Equals(foreignKey.Name));
            if (fk != null)
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Remove(fk);

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Add(foreignKey);

            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Remove(foreignKey);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to add foreign key!");
            }
        }

        public void AddIndex(string dbName, string tableName, string indexName, string columnName, string mongoID)
        {
            if (GetTable(dbName, tableName).PrimaryKeys.Any(pk => pk.Name.Equals(columnName)))
            {
                _logger.Error("Primary key is already an index!");
                throw new DataAccesException("Primary key is already an index!");
            }

            IndexFile? indFile = GetTable(dbName, tableName).IndexFiles.FirstOrDefault(ind => ind.Name.Equals(indexName));
            if (indFile != null)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.First(ind => ind.Name.Equals(indexName)).Attributes.Add(columnName);

                try
                {
                    _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
                    return;
                }
                catch (FileHandlerException ex)
                {
                    _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.First(ind => ind.Name.Equals(indexName)).Attributes.Remove(columnName);
                    _logger.Error($"Failed to write db data: {ex.Message}");
                    throw new DataAccesException("Failed to add index!");
                }
            }

            indFile = new IndexFile();
            indFile.Name = indexName;
            indFile.Attributes.Add(columnName);
            indFile.MongoID = mongoID;
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Add(indFile);

            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
                return;
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Remove(indFile);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to add index!");
            }
        }

        public void DropIndex(string dbName, string tableName, IndexFile index)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Remove(index);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Add(index);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataResourceException("Failed to drop table!");
            }
        }
        public void AddTable(string dbName, string tableId, Table table)
        {
            table.MongoID = tableId;
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.Add(table);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.Remove(table);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataResourceException("Failed to create table!");
            }
        }

        public void DropDatabase(DataBase database)
        {
            _dataBases.Remove(database);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.Add(database);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to delete db!");
            }
        }

        public void DropTable(string dbName, Table table)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.Remove(table);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.Add(table);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataResourceException("Failed to drop table!");
            }
        }

        public bool ExistsColumn(string dbName, string tableName, string columnName)
        {
            if (!ExistsDb(dbName))
                return false;

            if (!ExistsTable(dbName, tableName))
                return false;

            DataBase db = _dataBases.First(db => db.Name.Equals(dbName));
            Table table = db.Tables.First(t => t.Name.Equals(tableName));

            if (table.Columns.Any(c => c.Name.Equals(columnName)))
            {
                return true;
            }

            return false;
        }
        public bool ExistsForeignKey(string dbName, string tableName, string foreignKey)
        {
            if (!ExistsDb(dbName))
                return false;

            if (!ExistsTable(dbName, tableName))
                return false;

            DataBase db = _dataBases.First(db => db.Name.Equals(dbName));
            Table table = db.Tables.First(t => t.Name.Equals(tableName));

            if (table.ForeignKeys.Any(fk => fk.Name.Equals(foreignKey)))
            {
                return true;
            }

            return false;
        }

        public bool ExistsIndex(string dbName, string tableName, string indexName)
        {
            if (!ExistsDb(dbName))
                return false;

            if (!ExistsTable(dbName, tableName))
                return false;

            DataBase db = _dataBases.First(db => db.Name.Equals(dbName));
            Table table = db.Tables.First(t => t.Name.Equals(tableName));

            if (table.IndexFiles.Any(iFile => iFile.Name.Equals(indexName)))
            {
                return true;
            }

            return false;
        }

        public bool ExistsDb(string dbName)
        {
            if (_dataBases.Any((t) => (t.Name.Equals(dbName))))
            {
                return true;
            }
            return false;
        }

        public bool ExistsTable(string dbName, string tableName)
        {
            if (!ExistsDb(dbName))
                return false;

            DataBase db = _dataBases.First(db => db.Name.Equals(dbName));
            if (db.Tables.Any(t => t.Name.Equals(tableName)))
            {
                return true;
            }
            return false;
        }
        // return the column names
        public string[] GetColumns(string dbName, string tableName)
        {
            Table table = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName));

            string[] columns = new string[table.Columns.Count];
            int index = 0;
            foreach (var pk in table.PrimaryKeys)
            {
                columns[index] = pk.Name;
                index++;
            }

            foreach (var column in table.Columns)
            {
                if (!table.PrimaryKeys.Any(pk => pk.Name.Equals(column.Name)))
                {
                    columns[index] = column.Name;
                    index++;
                }
            }

            return columns;
        }
        // return all table of all db
        public List<string[]> GetDBData()
        {
            List<string[]> data = new List<string[]>();

            foreach (var db in _dataBases)
            {
                string[] dbData = new string[db.Tables.Count + 1];
                dbData[0] = db.Name;
                for (int i = 0; i < db.Tables.Count; i++)
                {
                    dbData[i + 1] = db.Tables[i].Name;
                }
                data.Add(dbData);
            }

            return data;
        }

        public List<string[]> GetForeignKeys(string dbName, string tableName)
        {
            List<string[]> fkList = new List<string[]>();

            Table table = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName));
            foreach (var fk in table.ForeignKeys)
            {
                string[] data = [fk.Name, fk.AttributeName, fk.RefTableName, fk.RefAttributeName];
                fkList.Add(data);
            }

            return fkList;
        }

        public List<string> GetIndexData(string dbName, string tableName)
        {
            List<string> result = [];

            List<IndexFile> indexList = GetTable(dbName, tableName).IndexFiles;

            foreach (var index in indexList)
            {
                string data = index.Name;
                foreach (string col in index.Attributes)
                {
                    data = data + "^" + col;
                }
                result.Add(data);
            }

            return result;
        }

        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName)
        {
            Table table = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName));

            List<string[]> columnList = new List<string[]>();
            foreach (var col in table.Columns)
            {
                PrimaryKey? pKey = table.PrimaryKeys.FirstOrDefault(pk => pk.Name.Equals(col.Name), new PrimaryKey());
                bool isUnique = table.UniqueKeys.Contains(col.Name);
                string[] columnData =
                [
                    col.Name,
                    col.Type,
                    string.IsNullOrEmpty(pKey.Name) ? "--" : "1",
                    col.NotNull ? "1" : "--",
                    col.DefaultVal,
                    pKey.PKIdentity.Seed.ToString(),
                    pKey.PKIdentity.Step.ToString(),
                    isUnique ? "1" : "--",
                    col.Check,
                ];
                columnList.Add(columnData);
            }

            return columnList;
        }

        public string GetTableMongoId(string dbName, string tableName)
        {
            return _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).MongoID;
        }

        // returns the list of the tables of a db
        public string[] GetTables(string dbName)
        {
            DataBase database = _dataBases.First(db => db.Name.Equals(dbName));
            
            string[] tables = new string[database.Tables.Count];
            for (int i = 0; i < database.Tables.Count; i++)
            {
                tables[i] = database.Tables[i].Name;
            }

            return tables;
        }

        public Table GetTable(string dbName, string tableName)
        {
            return _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName));
        }

        public Column GetColumn(string dbName, string tableName, string columnName)
        {
            return _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.First(c => c.Name.Equals(columnName));
        }

        public IndexFile GetIndexFile(string dbName, string tableName, string indexFileName)
        {
            return _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.First(iFile => iFile.Name.Equals(indexFileName));
        }

        public void UpdateInnerSeed(string dbName, string tableName, int value)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).PrimaryKeys.First(pK => pK.PKIdentity.Seed != 0).PKIdentity.InnerSeed = value;
            
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataResourceException("Failed to update inner seed for identity!");
            }
        }

        public List<int> GetColumnPostions(string dbName, string tableName, List<string> columnNames)
        {
            List<int> result = [];

            List<string> columns = GetColumns(dbName, tableName).ToList();
            foreach (string columnName in columnNames)
            {
                result.Add(columns.IndexOf(columnName));
            }

            return result;
        }
        public int GetNrPkeys(string dbName, string tableName)
        {
            return GetTable(dbName, tableName).PrimaryKeys.Count;
        }

        public void DropColumn(string dbName, string tableName, Column column)
        {
            Table table = GetTable(dbName, tableName);
            PrimaryKey? pKey = table.PrimaryKeys.FirstOrDefault(pK => pK.Name.Equals(column.Name));
            if (pKey != null && table.PrimaryKeys.Count - 1 == 0)
            {
                _logger.Error("Table cannot have 0 primary keys at removing a column!");
                throw new DataResourceException("Table cannot have 0 primary keys at removing a column!");
            }

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(column);
            if (pKey != null)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).PrimaryKeys.Remove(pKey);
            }
            if (table.UniqueKeys.Contains(column.Name))
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).UniqueKeys.Remove(column.Name);
            }

            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Add(column);
                if (pKey != null)
                {
                    _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).PrimaryKeys.Add(pKey);
                }
                if (table.UniqueKeys.Contains(column.Name))
                {
                    _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).UniqueKeys.Add(column.Name);
                }
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataResourceException("Failed to drop column!");
            }
        }
        public void DropForeignKey(string dbName, string tableName, ForeignKey foreignKey)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Remove(foreignKey);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Add(foreignKey);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataResourceException("Failed to drop foreign key!");
            }
        }

        public bool HasFkConstraint(string dbName, string tableName, string columnName)
        {
            List<Table> tables = _dataBases.First(db => db.Name.Equals(dbName)).Tables.FindAll(t => t.ForeignKeys.Count > 0);
            foreach (Table table in tables)
            {
                if (table.ForeignKeys.Any(fk => (fk.RefTableName.Equals(tableName) && fk.RefAttributeName.Equals(columnName)) || 
                    (table.Name.Equals(tableName) && fk.AttributeName.Equals(columnName))))
                    return true;
            }
            return false;
        }

        public List<string> GetPrimaryKeys(string dbName, string tableName)
        {
            return GetTable(dbName, tableName).PrimaryKeys.Select(pK => pK.Name).ToList();
        }

        public string HasIndexOnIt(string dbName, string tableName, string columnName)
        {
            IndexFile? ind = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.FirstOrDefault(ind => ind.Attributes.Contains(columnName));
            if (ind == null)
                return null;
            return ind.Name;
        }
    }
}
