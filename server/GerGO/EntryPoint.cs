namespace GerGO
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {
            Server.InitializeServer("192.168.148.83", 12000);

            Server.Start();
            Server.Run();
        }
    }
}
