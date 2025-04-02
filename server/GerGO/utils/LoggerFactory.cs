namespace GerGO.Utils
{
    class LoggerFactory
    {
        private static ILogger loggerInstance = new ConsoleLogger();
        public static ILogger GetLogger()
        {
            return loggerInstance;
        }
    }
}
