using GerGO.models;

namespace GerGO.dataresource
{
    class MapResourceManager : ResourceManager
    {
        private Dictionary<string, DataBase> _dataBases;

        public MapResourceManager()
        {
            _dataBases = new Dictionary<string, DataBase>();
        }
    }
}
