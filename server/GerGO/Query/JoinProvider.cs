using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;
using GerGO.Models;

namespace GerGO.Query
{
    class JoinProvider
    {
        private static IStoredDataManager _storedDataManager;
        private static IMetaDataManager _metaDataManager;
        private static string _dbName;

        public static void Initialize(string dbName, IStoredDataManager storedDataManager, IMetaDataManager metaDataManager)
        {
            _dbName = dbName;
            _storedDataManager = storedDataManager;
            _metaDataManager = metaDataManager;
        }

        public static List<string> IndexedNestedLoopJoin(List<string> pKeyListOuter, int indexBaseColumn, string outerMongoID, string innerIndColName, string innerIndMongoID, int pos)
        {
            List<string> result = [];

            //foreach (string key in pKeyListOuter)
            //{
            //    string pKey = key.Split('#')[indexBaseColumn];
            //    string val = _storedDataManager.GetFullRow(_dbName, outerMongoID, pKey).Split('^')[pos];
            //    List<string> pKeysInner = _storedDataManager.GetValue(innerIndColName, innerIndMongoID, val).Split('#').ToList();
            //    pKeysInner.ForEach(p => result.Add($"{key}#{p}"));
            //}

            return result;
        }

        public static List<string> NestedLoopJoin(string mongoId1, List<string> pKeyList1, int indexBaseTable, string mongoId2, List<string> pKeyList2, int pos1, int pos2)
        {
            List<string> results = [];
            //foreach (var pk1 in pKeyList1)
            //{
            //    string row1 = _storedDataManager.GetFullRow(_dbName, mongoId1, pk1.Split('#')[indexBaseTable]);
            //    string col1 = row1.Split('^')[pos1];
            //    foreach (var pk2 in pKeyList2)
            //    {
            //        string row2 = _storedDataManager.GetFullRow(_dbName, mongoId2, pk2);
            //        string col2 = row2.Split('^')[pos2];
            //        if (col1 == col2)
            //        {
            //            results.Add($"{pk1}#{pk2}");
            //        }
            //    }
            //}
            return results;
        }
    }
}
