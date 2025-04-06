using GerGO.DataAcces.MetaData.FileHandler;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;

namespace GerGO.DataAcces.MetaData
{
    class XmlMetaDataManager : IMetaDataManager
    {
        private ILogger _logger = LoggerFactory.GetLogger();

        private List<DataBase> _dataBases;
        private IFileHandler _fileHandler = FileHandlerFactory.GetHandler();
        private string _dbDataSourceFile;

        public XmlMetaDataManager()
        {
            _dbDataSourceFile = "Catalog.xml";
            _dataBases = _fileHandler.ReadDataBaseData(_dbDataSourceFile);
        }

        public void AddColumn(string dbName, string tableName, Column column)
        {
            Column? col = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.FirstOrDefault(c => c.Name.Equals(column.Name));
            if (col != null)
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(col);

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

        public void AddIndex(string dbName, string tableName, IndexFile index)
        {
            IndexFile? indFile = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.FirstOrDefault(ind => ind.Name.Equals(index.Name));
            if (indFile != null)
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Remove(indFile);

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Add(index);

            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Remove(index);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to add index!");
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

        public bool ExistsIndex(string dbName, string tableName, string indexName)
        {
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
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columns[i] = table.Columns[i].Name;
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
                    col.NotNull.ToString(),
                    col.DefaultVal,
                    col.PrimaryKey.ToString(),
                    col.PKIdentity.Seed.ToString(),
                    col.PKIdentity.Step.ToString(),
                    col.Unique.ToString(),
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
    }
}
