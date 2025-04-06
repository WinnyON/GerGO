namespace GerGO
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {
            Server.InitializeServer("172.30.244.238", 12000);

            Server.Start();
            Server.Run();
        }
    }
}
