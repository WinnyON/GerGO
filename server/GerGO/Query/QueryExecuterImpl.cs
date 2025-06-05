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
                List<string> rows;
                Table selectBaseTable = _metaDataManager.GetTable(selectData.DbName, selectData.TableName);
                List<string[]> whereClauses = selectData.WhereClauses.FindAll(clause => clause[1].Equals(selectData.TableName));
                rows = Selection(selectData.DbName, selectBaseTable, whereClauses);

                List<string> tablesOrder = [selectData.TableName];
                selectData.JoinTables.ForEach(jt => tablesOrder.Add(jt[3]));

                foreach (var joinClause in selectData.JoinTables)
                {
                    Table innerTable = _metaDataManager.GetTable(selectData.DbName, joinClause[3]);
                    whereClauses = selectData.WhereClauses.FindAll(clause => clause[1].Equals(joinClause[3]));
                    bool isUnique = innerTable.UniqueKeys.Contains(joinClause[4]);
                    if (isUnique)
                    {
                        rows = IndexedNestedLoopJoin(selectData.DbName, rows, tablesOrder.IndexOf(joinClause[1]), _metaDataManager.GetColumnPostions(selectData.DbName, joinClause[1], [joinClause[2]])[0], 
                            joinClause[3], joinClause[4], whereClauses);
                        continue;
                    }

                    ForeignKey? fKey = innerTable.ForeignKeys.FirstOrDefault(fk => fk.AttributeName.Equals(joinClause[4]), null);
                    if (fKey != null)
                    {
                        rows = IndexedNestedLoopJoin(selectData.DbName, rows, tablesOrder.IndexOf(joinClause[1]), _metaDataManager.GetColumnPostions(selectData.DbName, joinClause[1], [joinClause[2]])[0],
                            joinClause[3], fKey, whereClauses);
                        continue;
                    }

                    IndexFile? iFile = innerTable.IndexFiles.FirstOrDefault(iFile => iFile.Attributes.Count == 1 && iFile.Attributes[0].Equals(joinClause[4]), null);
                    if (iFile != null)
                    {
                        rows = IndexedNestedLoopJoin(selectData.DbName, rows, tablesOrder.IndexOf(joinClause[1]), _metaDataManager.GetColumnPostions(selectData.DbName, joinClause[1], [joinClause[2]])[0],
                            joinClause[3], iFile, whereClauses);
                        continue;
                    }

                    var innerRows = Selection(selectData.DbName, innerTable, whereClauses);
                    rows = NestedLoopJoin(selectData.DbName, rows, tablesOrder.IndexOf(joinClause[1]), innerRows, joinClause);
                }

                return rows;
            }
            catch (DataAccesException ex)
            {
                _logger.Error($"Failed to execute query: {ex.Message}");
                throw new QueryExecuterException($"Failed to execute query: {ex.Message}");
            }
        }

        private List<string> NestedLoopJoin(string dbName, List<string> outerRows, int outerIndex, List<string> innerRows, string[] joinClause)
        {
            List<string> rows = [];

            outerRows.ForEach(oRow =>
            {
                string oVal = oRow.Split('#')[outerIndex].Split('^')[_metaDataManager.GetColumnPostions(dbName, joinClause[1], [joinClause[2]])[0]];
                innerRows.ForEach(iRow =>
                {
                    string iVal = iRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, joinClause[3], [joinClause[4]])[0]];
                    if (oVal.Equals(iVal))
                    {
                        rows.Add($"{oRow}#{iRow}");
                    }
                });
            });

            return rows;
        }

        private List<string> IndexedNestedLoopJoin(string dbName, List<string> outerRows, int outerIndex, int index, string innerTableName, ForeignKey fKey, List<string[]> innerTableWheres)
        {
            List<string> resultSet = [];
            Table innerTable = _metaDataManager.GetTable(dbName, innerTableName);
            if (_metaDataManager.GetColumn(dbName, innerTableName, fKey.AttributeName).Type == "int")
            {
                outerRows.ForEach(row =>
                {
                    int fKeyVal = int.Parse(row.Split('#')[outerIndex].Split('^')[index]);
                    List<string> innerPKeys = _storedDataManager.GetKeysWhere<int>(dbName, $"{innerTableName}_{fKey.Name}", "=", fKeyVal);
                    List<string> innerRows;
                    if (innerTable.PrimaryKeys.Count == 1 &&
                    _metaDataManager.GetColumn(dbName, innerTableName, innerTable.PrimaryKeys[0].Name).Type == "int")
                    {
                        innerRows = _storedDataManager.GetRows<int>(dbName, innerTableName, innerPKeys.Select(k => int.Parse(k)).ToList());
                    }
                    else
                    {
                        innerRows = _storedDataManager.GetRows<string>(dbName, innerTableName, innerPKeys);
                    }

                    if (innerTableWheres.Count > 0)
                    {
                        innerRows = innerRows.FindAll(iRow => innerTableWheres.All(where => {
                            string val = iRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, innerTableName, [where[2]])[0]];
                            return Validator.Match(_metaDataManager.GetColumn(dbName, innerTableName, where[2]).Type, val, where[4], where[3]);
                        }));
                    }
                    innerRows.ForEach(iRow => resultSet.Add($"{row}#{iRow}"));
                });
            }
            else
            {
                outerRows.ForEach(row =>
                {
                    string fKeyVal = row.Split('#')[outerIndex].Split('^')[index];
                    List<string> innerPKeys = _storedDataManager.GetKeysWhere<string>(dbName, $"{innerTableName}_{fKey.Name}", "=", fKeyVal);
                    List<string> innerRows;
                    if (innerTable.PrimaryKeys.Count == 1 &&
                    _metaDataManager.GetColumn(dbName, innerTableName, innerTable.PrimaryKeys[0].Name).Type == "int")
                    {
                        innerRows = _storedDataManager.GetRows<int>(dbName, innerTableName, innerPKeys.Select(k => int.Parse(k)).ToList());
                    }
                    else
                    {
                        innerRows = _storedDataManager.GetRows<string>(dbName, innerTableName, innerPKeys);
                    }

                    if (innerTableWheres.Count > 0)
                    {
                        innerRows = innerRows.FindAll(iRow => innerTableWheres.All(where => {
                            string val = iRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, innerTableName, [where[2]])[0]];
                            return Validator.Match(_metaDataManager.GetColumn(dbName, innerTableName, where[2]).Type, val, where[4], where[3]);
                        }));
                    }
                    innerRows.ForEach(iRow => resultSet.Add($"{row}#{iRow}"));
                });
            }

            return resultSet;
        }

        private List<string> IndexedNestedLoopJoin(string dbName, List<string> outerRows, int outerIndex, int index, string innerTableName, string uKey, List<string[]> innerTableWheres)
        {
            List<string> resultSet = [];
            Table innerTable = _metaDataManager.GetTable(dbName, innerTableName);
            if (_metaDataManager.GetColumn(dbName, innerTableName, uKey).Type == "int")
            {
                outerRows.ForEach(row =>
                {
                    int uKeyVal = int.Parse(row.Split('#')[outerIndex].Split('^')[index]);
                    string innerPKey = _storedDataManager.GetKeysWhere<int>(dbName, $"{innerTableName}_{uKey}_uniquekey", "=", uKeyVal)[0];
                    string innerRow;
                    if (innerTable.PrimaryKeys.Count == 1 &&
                    _metaDataManager.GetColumn(dbName, innerTableName, innerTable.PrimaryKeys[0].Name).Type == "int")
                    {
                        innerRow = _storedDataManager.GetRows<int>(dbName, innerTableName, [int.Parse(innerPKey)])[0];
                    }
                    else
                    {
                        innerRow = _storedDataManager.GetRows<string>(dbName, innerTableName, [innerPKey])[0];
                    }

                    if (innerTableWheres.Count > 0)
                    {
                        if (!innerTableWheres.All(where => {
                            string val = innerRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, innerTableName, [where[2]])[0]];
                            return Validator.Match(_metaDataManager.GetColumn(dbName, innerTableName, where[2]).Type, val, where[4], where[3]);
                        }))
                        {
                            resultSet.Add($"{row}#{innerRow}");
                        }
                    }
                    else
                    {
                        resultSet.Add($"{row}#{innerRow}");
                    }
                });
            }
            else
            {
                outerRows.ForEach(row =>
                {
                    string uKeyVal = row.Split('#')[outerIndex].Split('^')[index];
                    string innerPKey = _storedDataManager.GetKeysWhere<string>(dbName, $"{innerTableName}_{uKey}_uniquekey", "=", uKeyVal)[0];
                    string innerRow;
                    if (innerTable.PrimaryKeys.Count == 1 &&
                    _metaDataManager.GetColumn(dbName, innerTableName, innerTable.PrimaryKeys[0].Name).Type == "int")
                    {
                        innerRow = _storedDataManager.GetRows<int>(dbName, innerTableName, [int.Parse(innerPKey)])[0];
                    }
                    else
                    {
                        innerRow = _storedDataManager.GetRows<string>(dbName, innerTableName, [innerPKey])[0];
                    }

                    if (innerTableWheres.Count > 0)
                    {
                        if (!innerTableWheres.All(where => {
                            string val = innerRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, innerTableName, [where[2]])[0]];
                            return Validator.Match(_metaDataManager.GetColumn(dbName, innerTableName, where[2]).Type, val, where[4], where[3]);
                        }))
                        {
                            resultSet.Add($"{row}#{innerRow}");
                        }
                    }
                    else
                    {
                        resultSet.Add($"{row}#{innerRow}");
                    }
                });
            }

            return resultSet;
        }
        private List<string> IndexedNestedLoopJoin(string dbName, List<string> outerRows, int outerIndex, int index, string innerTableName, IndexFile iFile, List<string[]> innerTableWheres)
        {
            List<string> resultSet = [];
            Table innerTable = _metaDataManager.GetTable(dbName, innerTableName);
            if (_metaDataManager.GetColumn(dbName, innerTableName, iFile.Attributes[0]).Type == "int")
            {
                outerRows.ForEach(row =>
                {
                    int indexVal = int.Parse(row.Split('#')[outerIndex].Split('^')[index]);
                    List<string> innerPKeys = _storedDataManager.GetKeysWhere<int>(dbName, $"{innerTableName}_{iFile.Name}", "=", indexVal);
                    List<string> innerRows;
                    if (innerTable.PrimaryKeys.Count == 1 &&
                    _metaDataManager.GetColumn(dbName, innerTableName, innerTable.PrimaryKeys[0].Name).Type == "int")
                    {
                        innerRows = _storedDataManager.GetRows<int>(dbName, innerTableName, innerPKeys.Select(k => int.Parse(k)).ToList());
                    }
                    else
                    {
                        innerRows = _storedDataManager.GetRows<string>(dbName, innerTableName, innerPKeys);
                    }

                    if (innerTableWheres.Count > 0)
                    {
                        innerRows = innerRows.FindAll(iRow => innerTableWheres.All(where => {
                            string val = iRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, innerTableName, [where[2]])[0]];
                            return Validator.Match(_metaDataManager.GetColumn(dbName, innerTableName, where[2]).Type, val, where[4], where[3]);
                        }));
                    }
                    innerRows.ForEach(iRow => resultSet.Add($"{row}#{iRow}"));
                });
            }
            else
            {
                outerRows.ForEach(row =>
                {
                    string indexVal = row.Split('#')[outerIndex].Split('^')[index];
                    List<string> innerPKeys = _storedDataManager.GetKeysWhere<string>(dbName, $"{innerTableName}_{iFile.Name}", "=", indexVal);
                    List<string> innerRows;
                    if (innerTable.PrimaryKeys.Count == 1 &&
                    _metaDataManager.GetColumn(dbName, innerTableName, innerTable.PrimaryKeys[0].Name).Type == "int")
                    {
                        innerRows = _storedDataManager.GetRows<int>(dbName, innerTableName, innerPKeys.Select(k => int.Parse(k)).ToList());
                    }
                    else
                    {
                        innerRows = _storedDataManager.GetRows<string>(dbName, innerTableName, innerPKeys);
                    }

                    if (innerTableWheres.Count > 0)
                    {
                        innerRows = innerRows.FindAll(iRow => innerTableWheres.All(where => {
                            string val = iRow.Split('^')[_metaDataManager.GetColumnPostions(dbName, innerTableName, [where[2]])[0]];
                            return Validator.Match(_metaDataManager.GetColumn(dbName, innerTableName, where[2]).Type, val, where[4], where[3]);
                        }));
                    }
                    innerRows.ForEach(iRow => resultSet.Add($"{row}#{iRow}"));
                });
            }

            return resultSet;
        }

        private List<string> Selection(string dbName, Table table, List<string[]> whereClauses)
        {
            List<string> pKeys = [];
            if (whereClauses == null || whereClauses.Count == 0)
                pKeys = _storedDataManager.GetAllKeys(dbName, table.Name);

            List<string[]> clausesToCheck = [];
            foreach (var whereClause in whereClauses)
            {
                bool isPkey = table.PrimaryKeys.Any(pk => pk.Name.Equals(whereClause[2]));
                if (isPkey)
                {
                    List<string> resPKeys = [];
                    if (table.PrimaryKeys.Count == 1 && _metaDataManager.GetColumn(dbName, whereClause[1], whereClause[2]).Type == "int")
                    {
                        resPKeys = _storedDataManager.Get_idWhere<int>(dbName, table.Name, whereClause[3], int.Parse(whereClause[4]));
                    }
                    else
                    {
                        resPKeys = _storedDataManager.Get_idWhere<string>(dbName, table.Name, whereClause[3], whereClause[4]);
                    }
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();

                    continue;
                }

                bool isUniqueKey = table.UniqueKeys.Contains(whereClause[2]);
                if (isUniqueKey)
                {
                    List<string> resPKeys = [];
                    if (_metaDataManager.GetColumn(dbName, table.Name, whereClause[2]).Type == "int")
                    {
                        resPKeys = _storedDataManager.GetKeysWhere<int>(dbName, $"{table.Name}_{whereClause[2]}_uniquekey", whereClause[3], int.Parse(whereClause[4]));
                    }
                    else
                    {
                        resPKeys = _storedDataManager.GetKeysWhere<string>(dbName, $"{table.Name}_{whereClause[2]}_uniquekey", whereClause[3], whereClause[4]);
                    }
                    resPKeys = _storedDataManager.GetKeysWhere(dbName, $"{table.Name}_{whereClause[2]}_uniquekey", whereClause[3], whereClause[4]);
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();

                    continue;
                }

                IndexFile? iFile = table.IndexFiles.FirstOrDefault(iFile => iFile.Attributes.Contains(whereClause[2]), null);
                if (iFile != null)
                {
                    List<string> resPKeys = [];
                    if (iFile.Attributes.Count == 1 && _metaDataManager.GetColumn(dbName, table.Name, iFile.Attributes[0]).Type == "int")
                    {
                        resPKeys = _storedDataManager.GetKeysWhere<int>(dbName, $"{table.Name}_{iFile.Name}", whereClause[3], int.Parse(whereClause[4]));
                    }
                    else
                    {
                        resPKeys = _storedDataManager.GetKeysWhere<string>(dbName, $"{table.Name}_{iFile.Name}", whereClause[3], whereClause[4]);
                    }
                    if (pKeys.Count == 0)
                        pKeys = resPKeys;
                    else
                        pKeys = pKeys.Intersect(resPKeys).ToList();
                    continue;
                }

                ForeignKey? fKey = table.ForeignKeys.FirstOrDefault(fKey => fKey.AttributeName.Equals(whereClause[2]), null);
                if (fKey != null)
                {
                    List<string> resPKeys = [];
                    if (_metaDataManager.GetColumn(dbName, table.Name, fKey.AttributeName).Type == "int")
                    {
                        resPKeys = _storedDataManager.GetKeysWhere<int>(dbName, $"{table.Name}_{fKey.Name}", whereClause[3], int.Parse(whereClause[4]));
                    }
                    else
                    {
                        resPKeys = _storedDataManager.GetKeysWhere<string>(dbName, $"{table.Name}_{fKey.Name}", whereClause[3], whereClause[4]);
                    }
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


            if (table.PrimaryKeys.Count == 1 && _metaDataManager.GetColumn(dbName, table.Name, table.PrimaryKeys[0].Name).Type == "int")
            {
                return _storedDataManager.GetRows<int>(dbName, table.Name, pKeys.Select(pk => int.Parse(pk)).ToList());
            }

            return _storedDataManager.GetRows<string>(dbName, table.Name, pKeys);
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
