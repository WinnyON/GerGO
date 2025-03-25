namespace GerGO.Utils
{
    class LoggerFactory
    {
        private static Logger loggerInstance = new ConsoleLogger();
        public static Logger GetLogger()
        {
            return loggerInstance;
        }
    }
}
