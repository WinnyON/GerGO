namespace GerGO.DataAcces.MetaData
{
    class MetaDataManagerFactory
    {
        private static IMetaDataManager _metaDataManager = null;
        public static IMetaDataManager GetMetaDataManager()
        {
            if (_metaDataManager == null)
            {
                _metaDataManager = new XmlMetaDataManager();
            }

            return _metaDataManager;
        }
    }
}
