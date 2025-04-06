namespace GerGO.Utils
{
    class LoggerFactory
    {
        private static readonly ILogger loggerInstance = new ConsoleLogger();
        public static ILogger GetLogger()
        {
            return loggerInstance;
        }
    }
}
