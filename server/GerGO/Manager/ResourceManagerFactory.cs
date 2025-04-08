namespace GerGO.Manager
{
    class ResourceManagerFactory
    {
        private static IResourceManager? s_instance = null;

        private static readonly object s_instanceLock = new object();
        public static IResourceManager GetInstance()
        {
            lock (s_instanceLock)
            {
                if (s_instance == null)
                    s_instance = new ResourceManagerImpl();
            }

            return s_instance;
        }
    }
}
