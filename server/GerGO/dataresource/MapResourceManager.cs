using GerGO.Models;
using GerGO.Utils;
using System.Data.Common;

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

        public void AddColumn(string dbName, string tableName, Column column)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dbName), null);
            if (result == null)
            {
                _logger.Error("The database doesn't exists!");
                throw new DataResourceException("The database doesn't exists!");
            }

            Table resTable = result.Tables.FirstOrDefault((t) => t.Name.Equals(tableName), null);
            if (resTable == null)
            {
                _logger.Error("The table doesn't exists!");
                throw new DataResourceException("The table doesn't exists!");
            }

            Column resColumn = resTable.Columns.FirstOrDefault(c =>  c.Name.Equals(column.Name), null);
            if (resColumn != null)
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(resColumn);
            }

            try
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Add(column);
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).Columns.Remove(column);
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to drop table!");
            }
        }

        public void AddForeignKey(string dbName, string tableName, ForeignKey foreignKey)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dbName), null);
            if (result == null)
            {
                _logger.Error("The database doesn't exists!");
                throw new DataResourceException("The database doesn't exists!");
            }

            Table resTable = result.Tables.FirstOrDefault((t) => t.Name.Equals(tableName), null);
            if (resTable == null)
            {
                _logger.Error("The table doesn't exists!");
                throw new DataResourceException("The table doesn't exists!");
            }

            Column resColumn = resTable.Columns.FirstOrDefault(c => c.Name.Equals(foreignKey.AttributeName), null);
            if (resColumn == null)
            {
                _logger.Error("The attribute doesn't exists!");
                throw new DataResourceException("The attribute doesn't exists!");
            }

            Table refTable = result.Tables.FirstOrDefault((t) => t.Name.Equals(foreignKey.RefTableName), null);
            if (refTable == null)
            {
                _logger.Error("The referrenced table doesn't exists!");
                throw new DataResourceException("The referrenced table doesn't exists!");
            }

            Column refColumn = refTable.Columns.FirstOrDefault(c => c.Name.Equals(foreignKey.RefAttributeName), null);
            if (refColumn == null)
            {
                _logger.Error("The referrenced attribute doesn't exists!");
                throw new DataResourceException("The referrenced attribute doesn't exists!");
            }

            ForeignKey resFk = resTable.ForeignKeys.FirstOrDefault(fk => fk.Name.Equals(foreignKey.Name), null);
            if (resFk != null)
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Remove(resFk);
            }

            try
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Add(foreignKey);
                _fileHandler.WriteDataBaseData(_dbDataSourceFile, _dataBases);
            }
            catch (FileHandlerException ex)
            {
                _dataBases.First((db) => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName)).ForeignKeys.Remove(foreignKey);
                _logger.Error("Failed to write db data: " + ex.Message);
                throw new DataResourceException("Failed to drop table!");
            }
        }

        public List<string[]> GetTableData(string dbName, string tableName)
        {
            Table resTable = _dataBases.First(db => db.Name.Equals(dbName)).Tables.First(t => t.Name.Equals(tableName));

            List<string[]> columnList = new List<string[]>();
            foreach (var col in resTable.Columns)
            {
                string[] columnData = new string[8];
                columnData[0] = col.Name;
                columnData[1] = col.Type;
                columnData[2] = col.NotNull.ToString();
                columnData[3] = col.DefaultVal;
                columnData[4] = col.PrimaryKey.ToString();
                columnData[5] = col.Identity.ToString();
                columnData[6] = col.Unique.ToString();
                columnData[7] = col.Check;
                
                columnList.Add(columnData);
            }

            return columnList;
        }

        public bool ExistsTable(string dbName, string tableName)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dbName), null);
            if (result == null)
            {
                _logger.Warning("The database doesn't exists!");
                return false;
            }

            Table resTable = result.Tables.FirstOrDefault((t) => t.Name.Equals(tableName), null);
            if (resTable == null)
            {
                _logger.Warning("The table doesn't exists!");
                return false;
            }

            return true;
        }

        public string[] GetTables(string dbName)
        {
            DataBase result = _dataBases.FirstOrDefault((db) => db.Name.Equals(dbName), null);
            if (result == null)
            {
                _logger.Error("The database doesn't exists!");
                throw new DataResourceException("The database doesn't exists!");
            }

            string[] resTables = new string[result.Tables.Count];
            for (int i = 0; i < result.Tables.Count; i++)
            {
                resTables[i] = result.Tables[i].Name;
            }

            return resTables;
        }
    }
}
