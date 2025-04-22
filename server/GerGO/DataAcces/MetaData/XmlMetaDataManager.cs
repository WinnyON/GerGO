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
            _dbDataSourceFile = "Catalog.xml";
            _dataBases = _fileHandler.ReadDataBaseData(_dbDataSourceFile);
        }

        public void AddColumn(string dbName, string tableName, Column column)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Add(column);

            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(column);
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
            if (GetColumn(dbName, tableName, columnName).PrimaryKey)
            {
                _logger.Error("Primary key is already an index!");
                throw new DataAccesException("Primary key is already an index!");
            }

            IndexFile? indFile = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.FirstOrDefault(ind => ind.Name.Equals(indexName));
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
            if (!ExitsDb(dbName))
                return false;

            if (!ExitsTable(dbName, tableName))
                return false;

            DataBase? db = _dataBases.First(db => db.Name.Equals(dbName));
            Table? table = db.Tables.First(t => t.Name.Equals(tableName));

            if (table.Columns.Any(c => c.Name.Equals(columnName)))
            {
                return true;
            }

            return false;
        }
        public bool ExistsForeignKey(string dbName, string tableName, string foreignKey)
        {
            if (!ExitsDb(dbName))
                return false;

            if (!ExitsTable(dbName, tableName))
                return false;

            DataBase? db = _dataBases.First(db => db.Name.Equals(dbName));
            Table? table = db.Tables.First(t => t.Name.Equals(tableName));

            if (table.ForeignKeys.Any(fk => fk.Name.Equals(foreignKey)))
            {
                return true;
            }

            return false;
        }

        public bool ExistsIndex(string dbName, string tableName, string indexName)
        {
            if (!ExitsDb(dbName))
                return false;

            if (!ExitsTable(dbName, tableName))
                return false;

            DataBase? db = _dataBases.First(db => db.Name.Equals(dbName));
            Table? table = db.Tables.First(t => t.Name.Equals(tableName));

            if (table.IndexFiles.Any(iFile => iFile.Name.Equals(indexName)))
            {
                return true;
            }

            return false;
        }

        public bool ExitsDb(string dbName)
        {
            if (_dataBases.Any((t) => (t.Name.Equals(dbName))))
            {
                return true;
            }
            return false;
        }

        public bool ExitsTable(string dbName, string tableName)
        {
            if (!ExitsDb(dbName))
                return false;

            DataBase? db = _dataBases.First(db => db.Name.Equals(dbName));
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
            foreach (var column in  table.Columns)
            {
                if (column.PrimaryKey)
                {
                    columns[index] = column.Name;
                    index++;
                }
            }

            foreach (var column in table.Columns)
            {
                if (!column.PrimaryKey)
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

            List<IndexFile> indexList = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles;

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

        public string GetNextKey(string dbName, string tableName)
        {
            List<Column> columns = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns;

            int nrKeys = 0;
            List<Identity> identityList = [];
            foreach (var column in columns)
            {
                if (column.PrimaryKey)
                {
                    nrKeys++;
                    identityList.Add(column.PKIdentity);
                }
            }

            if (nrKeys == 0 || nrKeys > 1)
            {
                return string.Empty;
            }

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.First(c => c.PrimaryKey).PKIdentity.Seed += identityList[0].Step;
            _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);

            return $"{identityList[0].Seed + identityList[0].Step}";
        }

        // return the columns with constraints
        public List<string[]> GetTableData(string dbName, string tableName)
        {
            Table table = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName));

            List<string[]> columnList = new List<string[]>();
            foreach (var col in table.Columns)
            {
                string[] columnData =
                [
                    col.Name,
                    col.Type,
                    col.PrimaryKey ? "1" : "--",
                    col.NotNull ? "1" : "--",
                    col.DefaultVal,
                    col.PKIdentity.Seed.ToString(),
                    col.PKIdentity.Step.ToString(),
                    col.Unique ? "1" : "--",
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
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.First(c => c.PKIdentity.Seed != 0).PKIdentity.InnerSeed = value;
            
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
            return _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Count(c => c.PrimaryKey);
        }

        public void DropColumn(string dbName, string tableName, Column column)
        {
            if (column.PrimaryKey && GetNrPkeys(dbName, tableName) - 1 == 0)
            {
                _logger.Error("Table cannot have 0 primary keys at removing a column!");
                throw new DataResourceException("Table cannot have 0 primary keys at removing a column!");
            }

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(column);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Add(column);
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
    }
}
