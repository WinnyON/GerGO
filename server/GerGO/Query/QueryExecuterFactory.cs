using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;

namespace GerGO.Query
{
    class QueryExecuterFactory
    {
        private static IQueryExecuter? _executer = null;

        public static IQueryExecuter GetExecuter(IMetaDataManager metaDataManager, IStoredDataManager storedDataManager)
        {
            if (_executer == null)
            {
                _executer = new QueryExecuterImpl(metaDataManager, storedDataManager);
            }
            return _executer;
        }
    }
}
