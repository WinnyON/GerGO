using GerGO.DataAcces;
using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;

namespace GerGO.Query
{
    class QueryExecuterImpl : IQueryExecuter
    {
        private IMetaDataManager _metaDataManager;
        private IStoredDataManager _storedDataManager;
        private ILogger _logger = LoggerFactory.GetLogger();
        public QueryExecuterImpl(IMetaDataManager metaDataManager, IStoredDataManager storedDataManager)
        {
            _metaDataManager = metaDataManager;
            _storedDataManager = storedDataManager;
        }

        public List<string> ExecuteQuery(ref SelectData selData)
        {
            SelectData selectData = selData;
            if (!IsValidSelectData(ref selectData))
            {
                _logger.Error("Not valid select data!");
                throw new DataResourceException("Not valid select data!");
            }

            try
            {
                List<string[]> whereClauses = selectData.WhereClauses.FindAll(clause => clause[1].Equals(selectData.TableName));
                Table selectBaseTable = _metaDataManager.GetTable(selectData.DbName, selectData.TableName);
                List<string> pKeys = Selection(selectData.DbName, selectBaseTable, whereClauses);

                return pKeys;
            }
            catch (DataAccesException ex)
            {
                _logger.Error($"Failed to execute query: {ex.Message}");
                throw new QueryExecuterException($"Failed to execute query: {ex.Message}");
            }
        }


        private List<string> Selection(string dbName, Table table, List<string[]> whereClauses)
        {
            if (whereClauses == null || whereClauses.Count == 0)
                return _storedDataManager.GetAllKeys(dbName, table.Name);

            List<string> pKeys = [];
            List<string[]> clausesToCheck = [];
            foreach (var whereClause in whereClauses)
            {
                bool isPkey = table.PrimaryKeys.Any(pk => pk.Name.Equals(whereClause[2]));
                if (isPkey)
                {
                    List<string> resPKeys = _storedDataManager.Get_idWhere(dbName, table.Name, whereClause[3], whereClause[4]);
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();

                    continue;
                }

                bool isUniqueKey = table.UniqueKeys.Contains(whereClause[2]);
                if (isUniqueKey)
                {
                    List<string> resPKeys = _storedDataManager.GetKeysWhere(dbName, $"{table.Name}_{whereClause[2]}_uniquekey", whereClause[3], whereClause[4]);
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();

                    continue;
                }

                IndexFile? iFile = table.IndexFiles.FirstOrDefault(iFile => iFile.Attributes.Contains(whereClause[2]), null);
                if (iFile != null)
                {
                    List<string> resPKeys = _storedDataManager.GetKeysWhere(dbName, $"{table.Name}_{iFile.Name}", whereClause[3], whereClause[4]);
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();
                    continue;
                }

                ForeignKey? fKey = table.ForeignKeys.FirstOrDefault(fKey => fKey.AttributeName.Equals(whereClause[2]), null);
                if (fKey != null)
                {
                    List<string> resPKeys = _storedDataManager.GetKeysWhere(dbName, $"{table.Name}_{fKey.Name}", whereClause[3], whereClause[4]);
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();
                    continue;
                }

                clausesToCheck.Add(whereClause);
            }

            foreach (var clause in clausesToCheck)
            {
                int index = _metaDataManager.GetColumnPostions(dbName, clause[1], [clause[2]])[0] - table.PrimaryKeys.Count;
                Column col = _metaDataManager.GetColumn(dbName, table.Name, clause[2]);
                List<string> resPKeys = _storedDataManager.GetKeysWhereIter(dbName, table.Name, index, col.Type, clause[3], clause[4]);
                if (pKeys.Count == 0)
                    pKeys = resPKeys;
                else
                    pKeys = pKeys.Intersect(resPKeys).ToList();
            }

            return pKeys;
        }

        private bool IsValidSelectData(ref SelectData selectData)
        {
            if (!_metaDataManager.ExistsDb(selectData.DbName) || !_metaDataManager.ExistsTable(selectData.DbName, selectData.TableName))
                return false;

            if (selectData.Columns.Count == 1 && selectData.Columns[0][1] == "*")
            {
                selectData.Columns.Clear();
                var columns = _metaDataManager.GetColumns(selectData.DbName, selectData.TableName).ToList();
                foreach (var col in columns)
                {
                    selectData.Columns.Add(["25", selectData.TableName, col]);
                }

                foreach (var joinTable in selectData.JoinTables)
                {
                    columns = _metaDataManager.GetColumns(selectData.DbName, joinTable[1]).ToList();
                    foreach (var col in columns)
                    {
                        selectData.Columns.Add(["25", joinTable[1], joinTable[2]]);
                    }
                }
            }
            else
            {
                foreach (var col in selectData.Columns)
                {
                    if (col.Length != 3)
                        return false;
                    if (!_metaDataManager.ExistsTable(selectData.DbName, col[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, col[1], col[2]))
                        return false;
                }
            }

            foreach (var jTable in selectData.JoinTables)
            {
                if (jTable.Length != 5)
                    return false;
                if (!_metaDataManager.ExistsTable(selectData.DbName, jTable[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, jTable[1], jTable[2]))
                    return false;
                if (!_metaDataManager.ExistsTable(selectData.DbName, jTable[3]) || !_metaDataManager.ExistsColumn(selectData.DbName, jTable[3], jTable[4]))
                    return false;
            }

            List<string> operators = ["<", ">", "=", "<=", ">=", "<>"];
            foreach (var where in selectData.WhereClauses)
            {
                if (where.Length != 5)
                    return false;
                if (!_metaDataManager.ExistsTable(selectData.DbName, where[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, where[1], where[2]))
                    return false;
                if (!operators.Contains(where[3]))
                    return false;
            }

            foreach (var groupBy in selectData.GroupByClauses)
            {
                if (groupBy.Length != 3)
                    return false;
                if (!_metaDataManager.ExistsTable(selectData.DbName, groupBy[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, groupBy[1], groupBy[2]))
                    return false;
            }

            foreach (var having in selectData.HavingClauses)
            {
                if (having.Length != 5)
                    return false;
                if (!_metaDataManager.ExistsTable(selectData.DbName, having[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, having[1], having[2]))
                    return false;
                if (!operators.Contains(having[3]))
                    return false;
            }

            foreach (var orderBy in selectData.OrderByCluases)
            {
                if (orderBy.Length != 3)
                    return false;
                if (!_metaDataManager.ExistsTable(selectData.DbName, orderBy[1]) || !_metaDataManager.ExistsColumn(selectData.DbName, orderBy[1], orderBy[2]))
                    return false;
            }

            return true;
        }
    }
}
