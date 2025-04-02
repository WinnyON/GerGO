namespace GerGO.DataAcces.StoredData
{
    class StoredDataManagerFactory
    {
        private static IStoredDataManager _storedDataManager = null;
        public static IStoredDataManager GetStoredDataManager()
        {
            if (_storedDataManager == null)
            {
                _storedDataManager = new MongoDataManager();
            }
            return _storedDataManager;
        }
    }
}
