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
            return null;
        }
        //public List<string> ExecuteQuery(ref SelectData selData)
        //{
        //    SelectData selectData = selData;
        //    if (!IsValidSelectData(ref selectData))
        //    {
        //        _logger.Error("Not valid select data!");
        //        throw new DataResourceException("Not valid select data!");
        //    }

        //    try
        //    {
        //        // execute where caluses only on base table
        //        List<string[]> whereClauses = selectData.WhereClauses.FindAll(clause => clause[1].Equals(selectData.TableName));
        //        Table selectBaseTable = _metaDataManager.GetTable(selectData.DbName, selectData.TableName);
        //        List<string> pKeys = Selection(selectData.DbName, selectBaseTable, whereClauses);
        //        JoinProvider.Initialize(selectData.DbName, _storedDataManager, _metaDataManager);

        //        List<string> tableOrder = new string[selectData.JoinTables.Count + 1].ToList();
        //        tableOrder[0] = selectData.TableName;

        //        // join tables
        //        int ind = 1;
        //        foreach (var joinClause in selectData.JoinTables)
        //        {
        //            Table baseTable = _metaDataManager.GetTable(selectData.DbName, joinClause[1]);

        //            Table joinTable = _metaDataManager.GetTable(selectData.DbName, joinClause[3]);
        //            string outerMongoID = string.Empty;
        //            string innerColName = string.Empty;
        //            string innerMongoID = string.Empty;
        //            int pos = -1;

        //            // the inner table is the newly joind table
        //            UniqueKey? uKey = joinTable.UniqueKeys.FirstOrDefault(uKey => uKey.Column.Equals(joinClause[4]), null);
        //            ForeignKey? fKey = joinTable.ForeignKeys.FirstOrDefault(fKey => fKey.AttributeName.Equals(joinClause[4]), null);
        //            IndexFile? iFile = joinTable.IndexFiles.FirstOrDefault(iFile => iFile.Attributes.Contains(joinClause[4]), null);
        //            if (uKey != null)
        //            {
        //                outerMongoID = baseTable.MongoID;
        //                innerMongoID = uKey.MongoID;
        //                innerColName = $"{selectData.DbName}_{joinTable.Name}_uniquekeys";
        //                pos = _metaDataManager.GetColumnPostions(selectData.DbName, baseTable.Name, [joinClause[2]])[0];
        //            }
        //            else if (fKey != null)
        //            {
        //                outerMongoID = baseTable.MongoID;
        //                innerMongoID = fKey.MongoID;
        //                innerColName = $"{selectData.DbName}_{joinTable.Name}_foreignkeys";
        //                pos = _metaDataManager.GetColumnPostions(selectData.DbName, baseTable.Name, [joinClause[2]])[0];
        //            }
        //            else if (iFile != null)
        //            {
        //                outerMongoID = baseTable.MongoID;
        //                innerMongoID = iFile.MongoID;
        //                innerColName = $"{selectData.DbName}_{joinTable.Name}_indexfiles";
        //                pos = _metaDataManager.GetColumnPostions(selectData.DbName, baseTable.Name, [joinClause[2]])[0];
        //            }

        //            if (pos != -1)
        //            {
        //                tableOrder[ind] = joinTable.Name;
        //                List<string> pKeysOuter = pKeys.Select(pKey => pKey.Split('#')[tableOrder.IndexOf(baseTable.Name)]).ToList();
        //                pKeys = JoinProvider.IndexedNestedLoopJoin(pKeysOuter, tableOrder.IndexOf(baseTable.Name), outerMongoID, innerColName, innerMongoID, pos);
        //                continue;
        //            }

        //            // the inner table is the base table
        //            whereClauses = selectData.WhereClauses.FindAll(clause => clause[1].Equals(selectData.TableName));
        //            List<string> pKeysJoinTable = Selection(selectData.DbName, joinTable, whereClauses);

        //            uKey = baseTable.UniqueKeys.FirstOrDefault(uKey => uKey.Column.Equals(joinClause[2]), null);
        //            fKey = baseTable.ForeignKeys.FirstOrDefault(fKey => fKey.AttributeName.Equals(joinClause[2]), null);
        //            iFile = baseTable.IndexFiles.FirstOrDefault(iFile => iFile.Attributes.Contains(joinClause[2]), null);
        //            if (uKey != null)
        //            {
        //                outerMongoID = joinTable.MongoID;
        //                innerMongoID = uKey.MongoID;
        //                innerColName = $"{selectData.DbName}_{baseTable.Name}_uniquekeys";
        //                pos = _metaDataManager.GetColumnPostions(selectData.DbName, joinTable.Name, [joinClause[2]])[0];
        //            }
        //            else if (fKey != null)
        //            {
        //                outerMongoID = joinTable.MongoID;
        //                innerMongoID = fKey.MongoID;
        //                innerColName = $"{selectData.DbName}_{baseTable.Name}_foreignkeys";
        //                pos = _metaDataManager.GetColumnPostions(selectData.DbName, joinTable.Name, [joinClause[2]])[0];
        //            }
        //            else if (iFile != null)
        //            {
        //                outerMongoID = joinTable.MongoID;
        //                innerMongoID = iFile.MongoID;
        //                innerColName = $"{selectData.DbName}_{baseTable.Name}_indexfiles";
        //                pos = _metaDataManager.GetColumnPostions(selectData.DbName, joinTable.Name, [joinClause[2]])[0];
        //            }

        //            if (pos != -1)
        //            {
        //                int indTmp = tableOrder.IndexOf(baseTable.Name);
        //                tableOrder[ind] = baseTable.Name;
        //                tableOrder[indTmp] = joinTable.Name;
        //                pKeys = JoinProvider.IndexedNestedLoopJoin(pKeysJoinTable, 0, outerMongoID, innerColName, innerMongoID, pos);
        //                continue;
        //            }

        //            int pos1 = _metaDataManager.GetColumnPostions(selectData.DbName, baseTable.Name, [joinClause[2]])[0];
        //            int pos2 = _metaDataManager.GetColumnPostions(selectData.DbName, joinTable.Name, [joinClause[4]])[0];
        //            pKeys = JoinProvider.NestedLoopJoin(baseTable.MongoID, pKeys, tableOrder.IndexOf(baseTable.Name), joinTable.MongoID, pKeysJoinTable, pos1, pos2);
        //            tableOrder[ind] = joinTable.Name;
        //        }

        //        List<string> resultSet = [];

        //        // projection
        //        List<string> tables = selectData.Columns.Select(col => col[1]).ToList();
        //        List<string> tableMongoIds = selectData.Columns.Select(col => _metaDataManager.GetTableMongoId(selectData.DbName, col[1])).ToList();
        //        List<int> indeces = selectData.Columns.Select(col => _metaDataManager.GetColumnPostions(selectData.DbName, col[1], [col[2]])[0]).ToList();
        //        foreach (var joinedKey in pKeys)
        //        {
        //            List<string> keys = joinedKey.Split('#').ToList();
        //            string resRow = _storedDataManager.GetFullRow(selectData.DbName, tableMongoIds[0], keys[tableOrder.IndexOf(tables[0])]).Split('^')[indeces[0]];
        //            for (int i = 1; i < tables.Count; i++)
        //            {
        //                resRow = $"{resRow}^{_storedDataManager.GetFullRow(selectData.DbName, tableMongoIds[i], keys[tableOrder.IndexOf(tables[i])]).Split('^')[indeces[i]]}";
        //            }
        //            resultSet.Add(resRow);
        //        }


        //        return resultSet;
        //    }
        //    catch (DataAccesException ex)
        //    {
        //        _logger.Error($"Failed to execute query: {ex.Message}");
        //        throw new QueryExecuterException($"Failed to execute query: {ex.Message}");
        //    }
        //}



        private List<string> Selection(string dbName, Table table, List<string[]> whereClauses)
        {
            return null;
        }
        //private List<string> Selection(string dbName, Table table, List<string[]> whereClauses)
        //{
        //    if (whereClauses == null || whereClauses.Count == 0)
        //        return _storedDataManager.GetPrimaryKeys(dbName, table.MongoID);

        //    List<string> pKeys = [];
        //    List<string[]> clausesToCheck = [];
        //    foreach (var whereClause in whereClauses)
        //    {
        //        UniqueKey? uKey = table.UniqueKeys.FirstOrDefault(uKey => uKey.Column.Equals(whereClause[2]), null);
        //        if (uKey != null)
        //        {
        //            List<string> resPKeys = _storedDataManager.GetPrimaryKeysWhere($"{dbName}_{table.Name}_uniquekeys", uKey.MongoID, 0,
        //            _metaDataManager.GetColumn(dbName, table.Name, whereClause[2]).Type, whereClause[3], whereClause[4]);
        //            if (pKeys.Count == 0)
        //                pKeys = resPKeys;
        //            else
        //                pKeys = pKeys.Intersect(resPKeys).ToList();

        //            continue;
        //        }

        //        IndexFile? iFile = table.IndexFiles.FirstOrDefault(iFile => iFile.Attributes.Contains(whereClause[2]), null);
        //        if (iFile != null)
        //        {
        //            List<string> resPKeys = _storedDataManager.GetPrimaryKeysWhere($"{dbName}_{table.Name}_indexfiles", iFile.MongoID, iFile.Attributes.IndexOf(whereClause[2]),
        //            _metaDataManager.GetColumn(dbName, table.Name, whereClause[2]).Type, whereClause[3], whereClause[4]);
        //            if (pKeys.Count == 0)
        //                pKeys = resPKeys;
        //            else
        //                pKeys = pKeys.Intersect(resPKeys).ToList();
        //            continue;
        //        }

        //        ForeignKey? fKey = table.ForeignKeys.FirstOrDefault(fKey => fKey.AttributeName.Equals(whereClause[2]), null);
        //        if (fKey != null)
        //        {
        //            List<string> resPKeys = _storedDataManager.GetPrimaryKeysWhere($"{dbName}_{table.Name}_foreignkeys", fKey.MongoID, 0,
        //            _metaDataManager.GetColumn(dbName, table.Name, whereClause[2]).Type, whereClause[3], whereClause[4]);
        //            if (pKeys.Count == 0)
        //                pKeys = resPKeys;
        //            else
        //                pKeys = pKeys.Intersect(resPKeys).ToList();
        //            continue;
        //        }

        //        clausesToCheck.Add(whereClause);
        //    }

        //    foreach (var clause in clausesToCheck)
        //    {
        //        List<string> resPKeys = _storedDataManager.GetPrimaryKeysWhereAllRow(dbName, table.MongoID, _metaDataManager.GetColumnPostions(dbName, table.Name, [clause[2]])[0],
        //            _metaDataManager.GetColumn(dbName, table.Name, clause[2]).Type, clause[3], clause[4]);
        //        if (pKeys.Count == 0)
        //            pKeys = resPKeys;
        //        else
        //            pKeys = pKeys.Intersect(resPKeys).ToList();
        //    }

        //    return pKeys;
        //}

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
