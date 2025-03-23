namespace GerGO.DataResource
{
    class ResourceManagerFactory
    {
        private static ResourceManager s_instance = null;

        private static object s_instanceLock = new object();
        public static ResourceManager GetInstance()
        {
            lock (s_instanceLock)
            {
                if (s_instance == null)
                    s_instance = new MapResourceManager();
            }

            return s_instance;
        }
    }
}
