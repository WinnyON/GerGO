namespace GerGO.Utils
{
    class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + message);
            Console.ForegroundColor = defaultColor;
        }

        public void Info(string message)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("INFO: " + message);
            Console.ForegroundColor = defaultColor;
        }

        public void Warning(string message)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: " + message);
            Console.ForegroundColor = defaultColor;
        }
    }
}
