namespace GerGO
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {
            Server.InitializeServer("127.0.0.1", 12000);

            Server.Start();
            Server.Run();
        }
    }
}
