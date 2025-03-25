namespace GerGO.Utils
{
    class FileHandlerFactory
    {
        public static FileHandler GetHandler()
        {
            return new XMLFileHandler();
        }
    }
}
