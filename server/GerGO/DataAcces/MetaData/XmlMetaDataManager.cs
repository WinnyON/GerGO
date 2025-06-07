using GerGO.DataAcces.MetaData.FileHandler;
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

        public void AddIndex(string dbName, string tableName, IndexFile iFile)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Add(iFile);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).IndexFiles.Remove(iFile);
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
                throw new DataAccesException("Failed to drop table!");
            }
        }
        public void AddTable(string dbName, Table table)
        {
            _dataBases.First(db => db.Name.Equals(dbName)).Tables.Add(table);
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.Remove(table);
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to create table!");
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
                throw new DataAccesException("Failed to drop table!");
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
            if (_dataBases.Any((db) => (db.Name.Equals(dbName))))
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
                    pKey.IdentitySeed.ToString(),
                    pKey.IdentityStep.ToString(),
                    isUnique ? "1" : "--",
                    col.Check,
                ];
                columnList.Add(columnData);
            }

            return columnList;
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

        public ForeignKey GetForeignKey(string dbName, string tableName, string fkName)
        {
            return _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.First(fk => fk.Name.Equals(fkName));
        }

        public string GetPrimaryKey(string dbName, Table table, string row, List<string> columnNames)
        {
            if (table.PrimaryKeys.Count == 0)
                throw new DataAccesException("");

            if (table.PrimaryKeys.Count == 1 && table.PrimaryKeys[0].IdentitySeed != 0)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(table.Name)).PrimaryKeys[0].IdentityInnerSeed +=
                    table.PrimaryKeys[0].IdentityStep;

                return table.PrimaryKeys[0].IdentityInnerSeed.ToString();
            }

            string key = columnNames[columnNames.IndexOf(table.PrimaryKeys[0].Name)];
            for (int i = 1; i < table.PrimaryKeys.Count; i++)
            {
                key = key + "^" + columnNames[columnNames.IndexOf(table.PrimaryKeys[i].Name)];
            }
            return key;
        }

        public void WriteData()
        {
            try
            {
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);

            }
            catch (FileHandlerException ex)
            {
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to update inner seed for identity!");
            }
        }

        public string GetValuePart(Table table, string row, List<string> columnNames)
        {
            List<string> values = [];
            List<string> insertedRow = row.Split('^').ToList();
            foreach (var column in table.Columns)
            {
                if (table.PrimaryKeys.Any(pk => pk.Name.Equals(column.Name)))
                    continue;

                string colValue = insertedRow[columnNames.IndexOf(column.Name)];
                if (!string.IsNullOrEmpty(column.DefaultVal) && (colValue.Equals("0") || colValue.Equals("null") || colValue.Equals(string.Empty)))
                {
                    values.Add(column.DefaultVal);
                    continue;
                }

                if (column.NotNull)
                {
                    if (colValue.Equals("0") || colValue.Equals("null") || colValue.Equals(string.Empty))
                        throw new DataAccesException("");
                }

                try
                {
                    // type check
                    switch (column.Type)
                    {
                        case "int":
                            _ = int.Parse(colValue);
                            break;
                        case "float":
                            _ = float.Parse(colValue);
                            break;
                        case "bit":
                            _ = bool.Parse(colValue);
                            break;
                        case "date":
                            _ = DateTime.Parse(colValue);
                            break;
                        case "datetime":
                            _ = TimeSpan.Parse(colValue);
                            break;
                        case "string":
                            break;
                        default:
                            throw new DataAccesException("");
                    }
                }
                catch (Exception)
                {
                    throw new DataAccesException("");
                }

                if (!column.Check.Equals("--") && !string.IsNullOrEmpty(column.Check))
                {
                    string[] checkConst = column.Check.Split('^');
                    switch (checkConst[0])
                    {
                        case "=":
                        case "==":
                            if (colValue != checkConst[1]) throw new DataAccesException("");
                            break;
                        case ">":
                            if (Validator.IsLessOrEqual(colValue, checkConst[1], column.Type)) throw new DataAccesException("");
                            break;
                        case ">=":
                            if (Validator.IsLess(colValue, checkConst[1], column.Type)) throw new DataAccesException("");
                            break;
                        case "<":
                            if (Validator.IsGreaterOrEqual(colValue, checkConst[1], column.Type)) throw new DataAccesException("");
                            break;
                        case "<=":
                            if (Validator.IsGreater(colValue, checkConst[1], column.Type)) throw new DataAccesException("");
                            break;
                        default:
                            throw new DataAccesException("");
                    }
                }

                values.Add(colValue);
            }
            return string.Join('^', values);
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

        public void DropColumn(string dbName, string tableName, Column column)
        {
            Table table = GetTable(dbName, tableName);
            PrimaryKey? pKey = table.PrimaryKeys.FirstOrDefault(pK => pK.Name.Equals(column.Name), null);
            bool isUnique = table.UniqueKeys.Contains(column.Name);

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(column);
            if (pKey != null)
            {
                _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).PrimaryKeys.Remove(pKey);
            }
            if (isUnique)
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
                if (isUnique)
                {
                    _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).UniqueKeys.Add(column.Name);
                }
                _logger.Error($"Failed to write db data: {ex.Message}");
                throw new DataAccesException("Failed to drop column!");
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
                throw new DataAccesException("Failed to drop foreign key!");
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

        public List<string> GetReferingForeignKeys(string dbName, string tableName)
        {
            List<string> result = [];

            _dataBases.First(db => db.Name.Equals(dbName)).Tables.ForEach(t =>
            {
                t.ForeignKeys.FindAll(fk => fk.RefTableName.Equals(tableName)).ForEach(fk => result.Add($"{t.Name}_{fk.Name}"));
            });

            return result;
        }
    }
}
