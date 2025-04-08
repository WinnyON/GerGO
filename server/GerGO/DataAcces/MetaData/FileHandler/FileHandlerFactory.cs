using GerGO.Utils;

namespace GerGO.DataAcces.MetaData.FileHandler
{
    class FileHandlerFactory
    {
        public static IFileHandler GetHandler()
        {
            return new XMLFileHandler();
        }
    }
}
